using Microsoft.EntityFrameworkCore;
using MovieRecommender.Domain.Interfaces;
using MovieRecommender.Infrastructure.Data;
using MovieRecommender.Infrastructure.Repositories;
using DotNetEnv;
using System.IO;
using OpenAI;
using MovieRecommender.Application.Services;
using MovieRecommender.Application.Config;

var builder = WebApplication.CreateBuilder(args);

// Attempt to locate and load a .env file from the current directory or up to 4 parent directories.
// This helps when you run the app from a different working directory than the file location.
string? foundEnv = null;
var current = Directory.GetCurrentDirectory();
for (int i = 0; i < 5 && current != null; i++)
{
    var candidate = Path.Combine(current, ".env");
    if (File.Exists(candidate))
    {
        foundEnv = candidate;
        break;
    }
    var parent = Directory.GetParent(current);
    current = parent?.FullName;
}

if (foundEnv != null)
{
    Env.Load(foundEnv);
    Console.WriteLine($"Loaded .env from: {foundEnv}");
}
else
{
    Console.WriteLine("No .env file found in current or parent directories. Using environment variables.");
}

// Only use OPENAI_API_KEY. Load from environment (DotNetEnv already loaded .env if present).
var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
var tmdbApiKey = Environment.GetEnvironmentVariable("TMDB_API_KEY");
var tmdbLanguage = Environment.GetEnvironmentVariable("TMDB_LANGUAGE") ?? "pt-BR";

if (string.IsNullOrWhiteSpace(openAiApiKey))
    throw new Exception("A variável OPENAI_API_KEY não está configurada.");

if (string.IsNullOrWhiteSpace(tmdbApiKey))
    throw new Exception("A variável TMDB_API_KEY não está configurada.");

builder.Services.AddHttpClient<ExternalMovieService>(client =>
{
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton(new TmdbConfig { ApiKey = tmdbApiKey, Language = tmdbLanguage });
builder.Services.AddSingleton(new OpenAIClient(openAiApiKey));

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped(sp => new MovieRecommender.Application.Services.EmbeddingService(openAiApiKey));

builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<MovieRecommender.Application.Services.MovieService>();

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapControllers();

app.Run();