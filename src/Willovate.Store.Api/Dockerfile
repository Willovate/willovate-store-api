FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

COPY ["src/Willovate.Store.Api/Willovate.Store.Api.csproj", "src/Willovate.Store.Api/"]
RUN dotnet restore "src/Willovate.Store.Api/Willovate.Store.Api.csproj"

COPY . .
RUN dotnet publish "src/Willovate.Store.Api/Willovate.Store.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

USER $APP_UID
ENTRYPOINT ["dotnet", "Willovate.Store.Api.dll"]
