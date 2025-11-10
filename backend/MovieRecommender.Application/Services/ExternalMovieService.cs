using System.Text.Json;
using Microsoft.Extensions.Logging;
using MovieRecommender.Application.Config;

namespace MovieRecommender.Application.Services
{
    public class ExternalMovieService
    {
        private readonly HttpClient _httpClient;
        private readonly TmdbConfig _config;
        private readonly ILogger<ExternalMovieService> _logger;

        public ExternalMovieService(HttpClient httpClient, TmdbConfig config, ILogger<ExternalMovieService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<(string? PosterPath, string? BackdropPath)> GetMovieImagesAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return (null, null);

            var encodedTitle = Uri.EscapeDataString(title);
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.themoviedb.org/3/search/movie?query={encodedTitle}&language={_config.Language}");
            request.Headers.Add("Accept-Language", _config.Language);
            // Prefer Bearer header when the config already contains a bearer token (user may include the 'Bearer ' prefix)
            if (!string.IsNullOrWhiteSpace(_config.ApiKey))
            {
                var apiKey = _config.ApiKey.Trim();
                if (apiKey.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey.Substring(7));
                }
                   else if (apiKey.Length > 0 && apiKey.IndexOfAny(new[] { ' ', '\n', '\r' }) < 0 && apiKey.Contains('.'))
                {
                    // Heuristic: some v4 tokens contain dots — treat as bearer value
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                }
                else
                {
                    // fall back to query param (v3 style)
                    request.RequestUri = new Uri(request.RequestUri + $"&api_key={Uri.EscapeDataString(apiKey)}");
                }
            }

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("TMDB search failed (status {Status}). Body: {Body}", response.StatusCode, body);
                return (null, null);
            }

            var json = await response.Content.ReadAsStringAsync();
            try
            {
                var data = JsonSerializer.Deserialize<JsonElement>(json);
                if (!data.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array || results.GetArrayLength() == 0)
                    return (null, null);

                var movieData = results[0];

                string? poster = null;
                if (movieData.TryGetProperty("poster_path", out var posterEl) && posterEl.ValueKind == JsonValueKind.String)
                    poster = posterEl.GetString();

                string? backdrop = null;
                if (movieData.TryGetProperty("backdrop_path", out var backdropEl) && backdropEl.ValueKind == JsonValueKind.String)
                    backdrop = backdropEl.GetString();

                string? posterUrl = !string.IsNullOrWhiteSpace(poster) ? $"https://image.tmdb.org/t/p/w500{poster}" : null;
                string? backdropUrl = !string.IsNullOrWhiteSpace(backdrop) ? $"https://image.tmdb.org/t/p/original{backdrop}" : null;

                return (posterUrl, backdropUrl);
            }
            catch (JsonException jex)
            {
                _logger.LogWarning("Failed to parse TMDB search response for title {Title}: {Err}", title, jex.Message);
                return (null, null);
            }
        }

        public class ExternalMovieInfo
        {
            public int TmdbId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Overview { get; set; } = string.Empty;
            public string? PosterPath { get; set; }
            public string? BackdropPath { get; set; }
            public int ReleaseYear { get; set; }
            public string Genre { get; set; } = string.Empty;
            public double? Rating { get; set; }
            public string? Director { get; set; }
        }

        public async Task<List<ExternalMovieInfo>> GetPopularMoviesAsync(int count)
        {
            var results = new List<ExternalMovieInfo>();
            if (count <= 0) return results;

            int page = 1;
            int perPage = 20;
            int totalPages = (int)Math.Ceiling(count / (double)perPage);

            for (; page <= totalPages; page++)
            {
                var uri = $"https://api.themoviedb.org/3/movie/popular?page={page}&language={_config.Language}";
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add("Accept-Language", _config.Language);
                if (!string.IsNullOrWhiteSpace(_config.ApiKey) && _config.ApiKey.Contains('.'))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ApiKey);
                }
                else
                {
                    request.RequestUri = new Uri(request.RequestUri + $"&api_key={_config.ApiKey}");
                }

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("TMDB popular failed on page {Page} (status {Status}). Body: {Body}", page, response.StatusCode, body);
                    break;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("results", out var resultsElement))
                    break;

                foreach (var item in resultsElement.EnumerateArray())
                {
                    if (results.Count >= count) break;

                    var id = item.TryGetProperty("id", out var idEl) ? idEl.GetInt32() : 0;
                    var title = item.TryGetProperty("title", out var tEl) ? tEl.GetString() ?? string.Empty : string.Empty;
                    var overview = item.TryGetProperty("overview", out var oEl) ? oEl.GetString() ?? string.Empty : string.Empty;
                    var poster = item.TryGetProperty("poster_path", out var pEl) ? pEl.GetString() : null;
                    var backdrop = item.TryGetProperty("backdrop_path", out var bEl) ? bEl.GetString() : null;
                    var releaseDate = item.TryGetProperty("release_date", out var rEl) ? rEl.GetString() : null;

                    string? posterUrl = poster != null ? $"https://image.tmdb.org/t/p/w500{poster}" : null;
                    string? backdropUrl = backdrop != null ? $"https://image.tmdb.org/t/p/original{backdrop}" : null;

                    int year = 0;
                    if (!string.IsNullOrWhiteSpace(releaseDate) && DateTime.TryParse(releaseDate, out var dt))
                        year = dt.Year;

                    var info = new ExternalMovieInfo
                    {
                        TmdbId = id,
                        Title = title,
                        Overview = overview,
                        PosterPath = posterUrl,
                        BackdropPath = backdropUrl,
                        ReleaseYear = year
                    };

                    bool hasPtTranslation = false;
                    try
                    {
                        var detailsUri = $"https://api.themoviedb.org/3/movie/{id}?language={_config.Language}&append_to_response=credits,translations";
                        var detailsReq = new HttpRequestMessage(HttpMethod.Get, detailsUri);
                        detailsReq.Headers.Add("Accept-Language", _config.Language);
                        if (!string.IsNullOrWhiteSpace(_config.ApiKey) && _config.ApiKey.Contains('.'))
                        {
                            detailsReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ApiKey);
                        }
                        else
                        {
                            detailsReq.RequestUri = new Uri(detailsReq.RequestUri + $"&api_key={_config.ApiKey}");
                        }

                        var detailsResp = await _httpClient.SendAsync(detailsReq);
                        if (detailsResp.IsSuccessStatusCode)
                        {
                            var detailsJson = await detailsResp.Content.ReadAsStringAsync();
                            using var ddoc = JsonDocument.Parse(detailsJson);

                            if (ddoc.RootElement.TryGetProperty("translations", out var translationsRoot) && translationsRoot.TryGetProperty("translations", out var translationsArray))
                            {
                                foreach (var tr in translationsArray.EnumerateArray())
                                {
                                    if (tr.TryGetProperty("iso_639_1", out var isoEl) && string.Equals(isoEl.GetString(), "pt", StringComparison.OrdinalIgnoreCase))
                                    {
                                        hasPtTranslation = true;
                                        if (tr.TryGetProperty("data", out var dataEl))
                                        {
                                            if (dataEl.TryGetProperty("title", out var transTitleEl) && !string.IsNullOrWhiteSpace(transTitleEl.GetString()))
                                                info.Title = transTitleEl.GetString() ?? info.Title;
                                            if (dataEl.TryGetProperty("overview", out var transOverviewEl) && !string.IsNullOrWhiteSpace(transOverviewEl.GetString()))
                                                info.Overview = transOverviewEl.GetString() ?? info.Overview;
                                        }
                                        break;
                                    }
                                }
                            }

                            if (!hasPtTranslation)
                            {
                                _logger.LogInformation("Skipping TMDB id {Id} because no Portuguese translation found.", id);
                            }
                            else
                            {
                                if (ddoc.RootElement.TryGetProperty("genres", out var genresEl))
                                {
                                    var names = new List<string>();
                                    foreach (var g in genresEl.EnumerateArray())
                                    {
                                        if (g.TryGetProperty("name", out var nameEl))
                                            names.Add(nameEl.GetString() ?? string.Empty);
                                    }
                                    info.Genre = string.Join(", ", names);
                                }

                                if (ddoc.RootElement.TryGetProperty("vote_average", out var voteEl))
                                {
                                    double v;
                                    if (voteEl.ValueKind == JsonValueKind.Number && voteEl.TryGetDouble(out v))
                                    {
                                        info.Rating = v;
                                    }
                                    else if (voteEl.ValueKind == JsonValueKind.String && double.TryParse(voteEl.GetString(), out v))
                                    {
                                        info.Rating = v;
                                    }
                                }

                                if (ddoc.RootElement.TryGetProperty("credits", out var creditsEl) && creditsEl.TryGetProperty("crew", out var crewEl))
                                {
                                    foreach (var crewItem in crewEl.EnumerateArray())
                                    {
                                        if (crewItem.TryGetProperty("job", out var jobEl) && jobEl.ValueKind == JsonValueKind.String && string.Equals(jobEl.GetString(), "Director", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (crewItem.TryGetProperty("name", out var nameEl) && nameEl.ValueKind == JsonValueKind.String)
                                            {
                                                info.Director = nameEl.GetString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to fetch details for TMDB id {Id}: {Err}", id, ex.Message);
                    }
                    if (hasPtTranslation)
                    {
                        results.Add(info);
                    }
                }

                if (results.Count >= count) break;
            }

            return results;
        }
    }
}
