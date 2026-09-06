# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Quincena.slnx ./
COPY src/Quincena.Api/Quincena.Api.csproj src/Quincena.Api/
COPY src/Quincena.Application/Quincena.Application.csproj src/Quincena.Application/
COPY src/Quincena.Domain/Quincena.Domain.csproj src/Quincena.Domain/
COPY src/Quincena.Infrastructure/Quincena.Infrastructure.csproj src/Quincena.Infrastructure/
RUN dotnet restore src/Quincena.Api/Quincena.Api.csproj

COPY src/ src/
RUN dotnet publish src/Quincena.Api/Quincena.Api.csproj -c Release -o /app --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production
# Render inyecta PORT en runtime; ASP.NET Core debe escuchar ahí (no en el 8080 por defecto).
ENTRYPOINT ["/bin/sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet Quincena.Api.dll"]
