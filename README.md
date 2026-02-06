# MovieRecommender

Movie recommendation project based on embeddings and semantic search, with backend in .NET (Web API) and frontend in Vite + React + TypeScript.

## Overview

* Backend: multi-project solution (.NET) with Application, Domain, Infrastructure and WebAPI layers. Generates embeddings (OpenAI) and persists movie metadata.
* Frontend: Vite + React + Tailwind app to consume the APIs (`api/movies/*` route).
* Integrations: TheMovieDB (TMDB) for metadata (poster, backdrop, director, rating) and OpenAI for embeddings.

This documentation describes how to configure, run and debug the project locally.

## Repository structure (summary)

* MovieRecommender.sln
* Application/ (application logic, DTOs, use cases)
* Domain/ (entities and interfaces)
* Infrastructure/ (persistence, repository, EF Core)
* WebAPI/ (ASP.NET Core project that exposes endpoints)
* frontend/ (Vite + React app)

## Requirements

* .NET 7/8/9 SDK (compatible with the project; use `dotnet --version`)
* Node.js 16+ and npm
* (Optional) dotnet-ef if you need to generate/update migrations locally

## Environment variables

Create a `.env` file in the root (`d:\Projetos\MovieRecommender\.env`) with the necessary keys. Minimum example:
```
OPENAI_API_KEY=sk-... # OpenAI key (required for embeddings)
TMDB_API_KEY=your_tmdb_api_key # TMDB key (optional, used in TMDB requests)
ConnectionStrings__DefaultConnection=Data Source=app.db
```

Notes:

* The backend automatically searches for a `.env` in parent folders when available.
* The `TmdbConfig.Language` setting is set to `pt-BR` by default — adjust if you want another language.

## Running the backend (Windows / PowerShell)

1. Open a PowerShell terminal in the repository root.
2. Restore and build:
```
cd D:\Projetos\MovieRecommender
dotnet restore
dotnet build
```

3. Run the API:
```
cd D:\Projetos\MovieRecommender\WebAPI
dotnet run
```

By default the API exposes routes at `/api/movies` (e.g., `POST /api/movies/search`).

### Migrations / database

The project uses Entity Framework Core with a migration already included (SQLite by default). To apply migrations locally:
```
cd D:\Projetos\MovieRecommender\Infrastructure
dotnet ef database update --project ..\WebAPI --startup-project ..\WebAPI
```

If you don't have `dotnet-ef` installed, install it globally:
```
dotnet tool install --global dotnet-ef
```

## Running the frontend

1. Open a PowerShell terminal and navigate to `frontend`.
```
cd D:\Projetos\MovieRecommender\frontend
npm install
npm run dev
```

The frontend consumes the API (adjust `src` if you need to change base URL / proxy).

## Main endpoints (backend)

* POST /api/movies/search

  + Body: { "Prompt": "search text" }
  + Returns: list (up to 5) of similar movies — fields: Id, Title, Description, Genre, Rating, Director, PosterPath, BackdropPath
  + Note: for security the embedding property is not sent in search responses.
* POST /api/movies/seed

  + Body: { "Titles": ["Movie A", "Movie B"] }
  + The route tries to fetch metadata from TMDB and persist the movies (with embeddings).
* POST /api/movies/seed/popular

  + Body: { "Count": 200 }
  + Seeds popular movies (queries TMDB and adds to database)
* DELETE /api/movies/admin/clear

  + Clears all movies (administrative use).

## Important technical notes

* Domain `Movie` fields currently use names that reflect database columns (e.g., `overview`, `poster_path`, `backdropt_path`). There is a TODO to normalize these names to C# convention and update migrations if you choose to rename.
* `ExternalMovieService` consumes the TMDB API and already applies heuristics to use Authorization Bearer vs query param. Also performs null-safe parsing of JSON responses.
* The search endpoint applies a fixed limit of 5 results (enforced in controller) to avoid very long responses.
