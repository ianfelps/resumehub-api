# ResumeHub API

Backend REST do **ResumeHub** — seu centro de comando de carreira. Cadastre uma vez todo o
inventário profissional (experiências, projetos, skills, idiomas, formação, cursos) e monte
**Perfis** curados por vaga, expostos publicamente por slug.

C# / .NET 10 · PostgreSQL (Supabase) via EF Core · ASP.NET Identity + JWT.

## Arquitetura

```
src/
  ResumeHub.Domain/          entidades + enums (sem dependências de infra)
  ResumeHub.Infrastructure/  DbContext, Identity, EF configs, migrations
  ResumeHub.Api/             controllers, services, DTOs, validators, auth, OpenAPI
```

Layered pragmático: controllers finos → services (com ownership) → EF Core. Sem CQRS/MediatR.

## Pré-requisitos

- .NET 10 SDK
- PostgreSQL (Supabase ou local)
- Ferramenta EF Core: `dotnet tool install --global dotnet-ef`

## Configuração (segredos via user-secrets)

Connection string e chave JWT **não** ficam no repositório. Configure por user-secrets:

```bash
cd src/ResumeHub.Api

# String do Postgres da Supabase (Settings → Database → Connection string → .NET / Npgsql)
dotnet user-secrets set "ConnectionStrings:Default" "Host=<host>;Port=5432;Database=postgres;Username=<user>;Password=<senha>;SSL Mode=Require;Trust Server Certificate=true"

# Chave JWT (>= 32 chars aleatórios)
dotnet user-secrets set "Jwt:Key" "<segredo-longo-aleatorio>"
```

`appsettings.json` traz só placeholders (localhost / chave fake) para o design-time funcionar;
user-secrets sobrescreve em runtime.

## Banco de dados

```bash
dotnet ef database update -p src/ResumeHub.Infrastructure -s src/ResumeHub.Api
```

Criar nova migration após mudar o domínio:

```bash
dotnet ef migrations add <Nome> -p src/ResumeHub.Infrastructure -s src/ResumeHub.Api -o Persistence/Migrations
```

## Rodar

```bash
dotnet run --project src/ResumeHub.Api
```

Docs interativas (Scalar) em ambiente Development: `http://localhost:<porta>/scalar`
(porta em `src/ResumeHub.Api/Properties/launchSettings.json`). Spec OpenAPI: `/openapi/v1.json`.

## Endpoints

| Área        | Rotas |
|-------------|-------|
| Auth        | `POST /api/auth/{register,login,refresh}` |
| Inventário  | CRUD em `/api/{experiences,projects,skills,languages,education,courses}` |
| Perfis      | CRUD `/api/profiles` + `PUT /api/profiles/{id}/items` (seleção/ordem) |
| Público     | `GET /api/public/{slug}` (currículo montado, só se `IsPublic`) |

Tudo exige JWT Bearer, exceto `/api/auth/*` e `/api/public/*`.

## Fluxo end-to-end (para validar)

1. `POST /api/auth/register` → recebe `accessToken` + `refreshToken`.
2. Com o Bearer, criar itens em `/api/experiences`, `/api/projects`, etc.
3. `POST /api/profiles` (gera slug único) e `PUT /api/profiles/{id}/items` selecionando ids.
4. `GET /api/public/{slug}` retorna o currículo montado, na ordem definida.
5. Ownership: token de outro usuário recebe 404 ao acessar recursos alheios.

## Roadmap

- Geração de PDF (fase 2).
- Atualização de portfólio público.
