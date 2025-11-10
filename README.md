# MovieRecommender

Projeto de recomendador de filmes baseado em embeddings e busca semântica, com backend em .NET (Web API) e frontend em Vite + React + TypeScript.

## Visão geral

- Backend: solução multi-projeto (.NET) com camadas Application, Domain, Infrastructure e WebAPI. Gera embeddings (OpenAI) e persiste metadados de filmes.
- Frontend: app Vite + React + Tailwind para consumir as APIs (rota `api/movies/*`).
- Integrações: TheMovieDB (TMDB) para metadados (poster, backdrop, diretor, avaliação) e OpenAI para embeddings.

Esta documentação descreve como configurar, rodar e depurar o projeto localmente.

## Estrutura do repositório (resumida)

- MovieRecommender.sln
- Application/ (lógica de aplicação, DTOs, use cases)
- Domain/ (entidades e interfaces)
- Infrastructure/ (persistência, repositório, EF Core)
- WebAPI/ (projecto ASP.NET Core que expõe endpoints)
- frontend/ (Vite + React app)

## Requisitos

- .NET 7/8/9 SDK (compatível com o projeto; use `dotnet --version`)
- Node.js 16+ e npm
- (Opcional) dotnet-ef se você precisar gerar/atualizar migrations localmente

## Variáveis de ambiente

Crie um arquivo `.env` na raiz (`d:\Projetos\MovieRecommender\.env`) com as chaves necessárias. Exemplo mínimo:

OPENAI_API_KEY=sk-...            # chave OpenAI (obrigatória para embeddings)
TMDB_API_KEY=your_tmdb_api_key    # chave TMDB (opcional, usada nas requisições ao TMDB)
ConnectionStrings__DefaultConnection=Data Source=app.db

Observações:
- O backend busca automaticamente um `.env` em pastas pai quando disponível.
- A configuração `TmdbConfig.Language` está definida como `pt-BR` por padrão — ajuste se quiser outro idioma.

## Rodando o backend (Windows / PowerShell)

1. Abra um terminal PowerShell na raiz do repositório.

2. Restaurar e build:

```powershell
cd D:\Projetos\MovieRecommender
dotnet restore
dotnet build
```

3. Rodar a API:

```powershell
cd D:\Projetos\MovieRecommender\WebAPI
dotnet run
```

Por padrão a API expõe rotas em `/api/movies` (ex.: `POST /api/movies/search`).

### Migrations / banco

O projeto usa Entity Framework Core com uma migration já incluída (SQLite por padrão). Para aplicar as migrations localmente:

```powershell
cd D:\Projetos\MovieRecommender\Infrastructure
dotnet ef database update --project ..\WebAPI --startup-project ..\WebAPI
```

Se você não tiver `dotnet-ef` instalado, instale globalmente:

```powershell
dotnet tool install --global dotnet-ef
```

## Rodando o frontend

1. Abra um terminal PowerShell e navegue até `frontend`.

```powershell
cd D:\Projetos\MovieRecommender\frontend
npm install
npm run dev
```

O frontend consome a API (ajuste `src` se precisar alterar URL base / proxy).

## Endpoints principais (backend)

- POST /api/movies/search
	- Body: { "Prompt": "texto de busca" }
	- Retorna: lista (até 5) de filmes semelhantes — fields: Id, Title, Description, Genre, Rating, Director, PosterPath, BackdropPath
	- Observação: por segurança a propriedade de embedding não é enviada nas respostas de busca.

- POST /api/movies/seed
	- Body: { "Titles": ["Movie A", "Movie B"] }
	- A rota tenta buscar metadados no TMDB e persistir os filmes (com embeddings).

- POST /api/movies/seed/popular
	- Body: { "Count": 200 }
	- Faz seed dos filmes populares (consulta TMDB e adiciona ao banco)

- DELETE /api/movies/admin/clear
	- Limpa todos os filmes (uso administrativo).

## Observações técnicas importantes

- Campos do Domain `Movie` atualmente usam nomes que refletem as colunas do banco (ex.: `overview`, `poster_path`, `backdropt_path`). Existe um TODO para normalizar estes nomes para a convenção C# e atualizar migrations se optar por renomear.
- `ExternalMovieService` consome a API do TMDB e já aplica heurísticas para usar Authorization Bearer vs query param. Também faz parsing null-safe dos JSONs de resposta.
- O endpoint de busca aplica um limite fixo de 5 resultados (forçado no controller) para evitar respostas muito longas.


