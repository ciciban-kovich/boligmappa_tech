# syntax=docker/dockerfile:1
# Multi-stage build. Run from the API project folder as the build context:
#   docker build -t boligmappa-api -f src/Boligmappa.Reminders.Api/Dockerfile src/Boligmappa.Reminders.Api
# (docker-compose.yml in the repo root does this for you.)

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first so the layer is cached when only source changes.
COPY *.csproj ./
RUN dotnet restore Boligmappa.Reminders.Api.csproj

COPY . ./
RUN dotnet publish Boligmappa.Reminders.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
# curl is only there so the compose healthcheck can hit /health.
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Boligmappa.Reminders.Api.dll"]
