# Arquitetura — ResumeHub API

Visão rápida da estrutura do backend e das regras que a mantêm consistente.

## Camadas

```
src/ResumeHub.Domain          Entidades e regras de domínio (zero dependências de infra)
src/ResumeHub.Application     Casos de uso, DTOs e ports (interfaces)
src/ResumeHub.Infrastructure  EF Core, PostgreSQL e JWT (implementações dos ports)
src/ResumeHub.Api             Controllers e configuração HTTP
tests/ResumeHub.Tests         Testes de domínio e de arquitetura
docs/architecture.md          Este documento
```

## Regra de dependência

```
Domain  ←  Application  ←  Infrastructure  ←  Api
                    ↑__________________________|
```

- **Domain** não referencia nenhuma outra camada.
- **Application** referencia só Domain. Define os *ports* (`IApplicationDbContext`,
  `ITokenService`, `ICurrentUser`) e os casos de uso que dependem deles.
- **Infrastructure** referencia Application + Domain e **implementa** os ports
  (`ResumeHubDbContext : IApplicationDbContext`, `TokenService`).
- **Api** referencia tudo, faz a composição (DI) e expõe HTTP. Implementa `ICurrentUser`
  lendo as claims do JWT.

Essa regra é verificada por testes (`tests/ResumeHub.Tests/Architecture`) usando NetArchTest —
o build de teste falha se alguém violar o sentido das setas.

## Ports e adapters

| Port (Application)      | Adapter (Infrastructure / Api)            | Para quê |
|-------------------------|-------------------------------------------|----------|
| `IApplicationDbContext` | `ResumeHubDbContext` (EF Core / Npgsql)   | Persistência; casos de uso não conhecem o EF concreto |
| `ITokenService`         | `TokenService` (JWT HS256)                | Emissão de access/refresh token |
| `ICurrentUser`          | `CurrentUser` (Api, lê claims do request) | Id do usuário autenticado |

`IApplicationDbContext` segue o padrão do template Clean Architecture do .NET: a Application
referencia o pacote `Microsoft.EntityFrameworkCore` (DbSet/IQueryable) mas **não** o projeto
Infrastructure. O DbContext concreto é registrado e exposto pelo port via DI.

## Fluxo de uma requisição

1. `Controller` (Api) recebe o request e chama um caso de uso da Application.
2. O caso de uso usa `ICurrentUser` para escopar dados ao dono e `IApplicationDbContext`
   para ler/gravar. Ownership é garantido na própria query.
3. Exceções de domínio (`NotFoundException`, `ConflictException`) sobem até o
   `GlobalExceptionHandler` (Api), que devolve `ProblemDetails` (RFC 7807).

## Configuração e segredos (dotenv)

- Segredos vêm de variáveis de ambiente, carregadas de um arquivo **`.env`** na inicialização
  via `DotNetEnv` (`Env.TraversePath().Load()` em `Program.cs` e no design-time factory).
- `.env` é git-ignored. Use `.env.example` como modelo. Chaves aninhadas usam `__`
  (ex.: `ConnectionStrings__Default`, `Jwt__Key`, `Minio__Endpoint`).
- `appsettings.json` traz só placeholders; o `.env` (ou user-secrets) sobrescreve em runtime.

## Testes

- **Domínio**: comportamento das entidades (`OwnedEntity` gera Id/timestamps) e utilidades
  puras (`SlugGenerator`).
- **Arquitetura**: a regra de dependência entre camadas.

Rodar: `dotnet test`.
