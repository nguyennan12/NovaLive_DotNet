FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY NovaLive.sln ./
COPY src/ ./src/
RUN dotnet restore src/NovaLive.Api/NovaLive.Api.csproj
RUN dotnet publish src/NovaLive.Api/NovaLive.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:10000

EXPOSE 10000
USER $APP_UID
ENTRYPOINT ["dotnet", "NovaLive.Api.dll"]
