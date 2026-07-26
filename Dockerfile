# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore isolado para aproveitar cache de camadas.
COPY Directory.Build.props ./
COPY OnibusExpress.sln ./
COPY src/OnibusExpress.Domain/*.csproj src/OnibusExpress.Domain/
COPY src/OnibusExpress.Application/*.csproj src/OnibusExpress.Application/
COPY src/OnibusExpress.Infrastructure/*.csproj src/OnibusExpress.Infrastructure/
COPY src/OnibusExpress.Api/*.csproj src/OnibusExpress.Api/
RUN dotnet restore src/OnibusExpress.Api/OnibusExpress.Api.csproj

COPY src/ src/
RUN dotnet publish src/OnibusExpress.Api/OnibusExpress.Api.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Usuário não-root (a imagem base já traz o usuário 'app', uid 64198).
USER app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "OnibusExpress.Api.dll"]
