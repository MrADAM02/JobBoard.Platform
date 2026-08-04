# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the whole solution up front - Directory.Build.props and every project
# reference (Api -> Application/Infrastructure -> Domain) live under this
# directory, so restore needs all of it, not just the Api project.
COPY . .
RUN dotnet restore JobBoard.sln

RUN dotnet publish src/JobBoard.Api/JobBoard.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# curl isn't in the base image - installed only for the HEALTHCHECK below.
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --start-period=20s \
    CMD curl --fail --silent http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "JobBoard.Api.dll"]
