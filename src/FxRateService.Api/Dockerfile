# ---- faza 1: build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Prvo samo project fajlovi — restore se kesira dok se zavisnosti ne promene.
COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/FxRateService.Core/FxRateService.Core.csproj src/FxRateService.Core/
COPY src/FxRateService.Infrastructure/FxRateService.Infrastructure.csproj src/FxRateService.Infrastructure/
COPY src/FxRateService.Api/FxRateService.Api.csproj src/FxRateService.Api/

RUN dotnet restore src/FxRateService.Api/FxRateService.Api.csproj

# Tek onda ceo izvorni kod.
COPY src/ src/

RUN dotnet publish src/FxRateService.Api/FxRateService.Api.csproj \
    -c Release -o /app --no-restore

# ---- faza 2: izvrsavanje ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FxRateService.Api.dll"]