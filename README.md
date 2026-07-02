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

## Segurança

Proteções ativas no backend:

- **Rate limiting**: limite global por IP (100 req/min) e limite estrito em `/api/auth/*`
  (10 req/min) contra brute-force/credential-stuffing. Excesso → `429` com `Retry-After`.
- **Lockout de conta**: 5 logins falhos bloqueiam a conta por 5 min (via ASP.NET Identity).
- **Security headers**: `X-Content-Type-Options`, `X-Frame-Options: DENY`, `Referrer-Policy`,
  `Content-Security-Policy` restritivo. `HSTS` em produção.
- **Validação de input** (FluentValidation): limites de tamanho e allowlist de esquema de URL
  (`http`/`https`) — bloqueia `javascript:`/`data:` que seriam renderizados como links no PDF.
- **Limite de body**: 1 MB (API só aceita JSON).
- **ForwardedHeaders**: honra `X-Forwarded-For`/`Proto` atrás de proxy/PaaS (IP real p/ rate
  limit; evita loop de HTTPS redirect).

### Segredos (rotação obrigatória antes do deploy)

O `.env` é git-ignored, mas se segredos reais já estiveram em disco/workspace compartilhado,
**trate-os como comprometidos e rotacione**:

- **Senha do Postgres (Supabase)** — troque no painel da Supabase e atualize `ConnectionStrings__Default`.
- **`Jwt__Key`** — gere uma nova chave aleatória (>= 32 chars). Uma chave vazada permite forjar
  tokens de qualquer usuário. Ex.: `openssl rand -hex 32`.

Em produção, **injete os segredos pelo cofre/variáveis de ambiente da plataforma** — nunca
faça commit do `.env` nem o inclua na imagem Docker (já está no `.dockerignore`).

## Deploy (Docker)

Imagem multi-stage (`Dockerfile`): build em `sdk:10.0`, runtime em `aspnet:10.0` (com libs de
fonte para o QuestPDF gerar PDF), rodando como usuário não-root na porta `8080`.

```bash
docker build -t resumehub-api .
docker run -p 8080:8080 \
  -e "ConnectionStrings__Default=Host=...;SSL Mode=VerifyFull" \
  -e "Jwt__Key=<segredo-aleatorio>" \
  -e "Jwt__Issuer=ResumeHub" -e "Jwt__Audience=ResumeHubClients" \
  -e "Cors__WebOrigin=https://seu-frontend.com" \
  resumehub-api
```

Variáveis de ambiente exigidas em produção:

| Variável | Nota |
|----------|------|
| `ConnectionStrings__Default` | string do Postgres (use `SSL Mode=VerifyFull`) |
| `Jwt__Key` | segredo aleatório >= 32 chars |
| `Jwt__Issuer` / `Jwt__Audience` | emissor/audiência do token |
| `Jwt__AccessTokenMinutes` / `Jwt__RefreshTokenDays` | opcional (defaults 15 / 7) |
| `Cors__WebOrigin` | origem real do frontend (não `localhost`) |
| `ASPNETCORE_ENVIRONMENT` | `Production` (default na imagem) |
| `ASPNETCORE_URLS` | `http://+:8080` (default na imagem) |

TLS normalmente é terminado pelo proxy da plataforma (Railway/Render/Fly/Azure). O
`ForwardedHeaders` já está configurado para funcionar atrás desse proxy.

### Render

O repositório traz um blueprint `render.yaml` (na raiz) que provisiona a API como Docker web
service. No dashboard: **New → Blueprint** apontando para o repo. O que ele faz:

- Build via `resumehub-api/Dockerfile` (contexto `resumehub-api`).
- Health check em `/health` (a app escuta a porta do `PORT` que a Render injeta).
- Gera automaticamente `Jwt__Key` (`generateValue: true`) — sem segredo no git.
- Pede no dashboard (`sync: false`): `ConnectionStrings__Default` (string da Supabase com
  `SSL Mode=VerifyFull`) e `Cors__WebOrigin` (origem real do frontend).

A app faz bind em `0.0.0.0:$PORT` quando `PORT` existe (Render/Railway); localmente cai no
default `:8080` da imagem. TLS é terminado no edge da Render — `ForwardedHeaders` cuida do resto.

## Roadmap

- Atualização de portfólio público.
