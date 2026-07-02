# syntax=docker/dockerfile:1

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (cached unless project/solution files change).
COPY global.json ./
COPY ResumeHub.slnx ./
COPY src/ResumeHub.Domain/ResumeHub.Domain.csproj src/ResumeHub.Domain/
COPY src/ResumeHub.Application/ResumeHub.Application.csproj src/ResumeHub.Application/
COPY src/ResumeHub.Infrastructure/ResumeHub.Infrastructure.csproj src/ResumeHub.Infrastructure/
COPY src/ResumeHub.Api/ResumeHub.Api.csproj src/ResumeHub.Api/
RUN dotnet restore src/ResumeHub.Api/ResumeHub.Api.csproj

# Copy the rest and publish.
COPY src/ src/
RUN dotnet publish src/ResumeHub.Api/ResumeHub.Api.csproj \
    -c Release -o /app --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# QuestPDF (SkiaSharp) needs native font libraries to render PDFs; without these the
# GET /api/profiles/{id}/pdf endpoint crashes at runtime on the slim base image.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 libfreetype6 \
    && rm -rf /var/lib/apt/lists/*

# Run as the non-root user provided by the aspnet image.
USER $APP_UID

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app ./
ENTRYPOINT ["dotnet", "ResumeHub.Api.dll"]
