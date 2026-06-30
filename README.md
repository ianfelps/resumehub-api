# ResumeHub API

Backend REST do **ResumeHub** — seu centro de comando de carreira. Cadastre uma vez todo o
inventário profissional (experiências, projetos, skills, idiomas, formação, cursos) e monte
**Perfis** curados por vaga, expostos publicamente por slug.

C# / .NET 10 · PostgreSQL (Supabase) via EF Core · ASP.NET Identity + JWT.

## Arquitetura

```
src/
  ResumeHub.Domain/          entidades + enums (sem dependências de infra)
  ResumeHub.Application/      casos de uso, DTOs e ports (interfaces)
  ResumeHub.Infrastructure/  EF Core, PostgreSQL e JWT (implementa os ports)
  ResumeHub.Api/             controllers e configuração HTTP
tests/
  ResumeHub.Tests/           testes de domínio e de arquitetura
docs/
  architecture.md            visão rápida da arquitetura
```

Regra de dependência: **Domain ← Application ← Infrastructure ← Api**. A Application define
ports (`IApplicationDbContext`, `ITokenService`, `IStorageService`, `ICurrentUser`); a
Infrastructure os implementa. Detalhes em [`docs/architecture.md`](docs/architecture.md).
A regra é verificada por testes (NetArchTest).

## Pré-requisitos

- .NET 10 SDK
- PostgreSQL (Supabase ou local)
- Ferramenta EF Core: `dotnet tool install --global dotnet-ef`

## Configuração (segredos via .env)

Connection string e chave JWT **não** ficam no repositório. A app carrega
um arquivo **`.env`** na inicialização (via `DotNetEnv`); o `.env` é git-ignored.

```bash
cp .env.example .env
# edite .env com a string da Supabase e um Jwt__Key aleatório (>= 32 chars)
```

Chaves aninhadas usam `__` (duplo underscore): `ConnectionStrings__Default`, `Jwt__Key`, etc.
O `.env` sobrescreve o `appsettings.json` (que traz só placeholders).
O `dotnet ef` também lê o `.env` (o design-time factory chama `Env.TraversePath().Load()`).

> Alternativa: `dotnet user-secrets` continua funcionando para quem preferir
> (`cd src/ResumeHub.Api && dotnet user-secrets set "Jwt:Key" "<segredo>"`).

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

## Testes

```bash
dotnet test
```

- **Domínio**: defaults de `OwnedEntity` (Id/timestamps) e `SlugGenerator`.
- **Arquitetura** (NetArchTest): garante a regra de dependência entre camadas — o build de
  teste falha se a Application passar a depender de Infrastructure/Api, etc.

## Roadmap

- Geração de PDF (fase 2).
- Atualização de portfólio público.
