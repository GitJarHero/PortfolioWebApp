FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Wichtig: Erzwinge die Ports aus launchSettings.json
ENV ASPNETCORE_URLS="http://+:80"
ENTRYPOINT ["dotnet", "PortfolioWebApp.dll"]