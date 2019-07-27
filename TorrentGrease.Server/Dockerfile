FROM mcr.microsoft.com/dotnet/core/aspnet:3.0.0-preview7-buster-slim AS base
WORKDIR /app
EXPOSE 5656

ENV ASPNETCORE_URLS=http://+:5656
HEALTHCHECK --interval=5s --timeout=10s --retries=3 CMD curl --fail http://localhost:5656/health || exit 1

FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview7-buster AS build
WORKDIR /src
COPY ["TorrentGrease.Server/TorrentGrease.Server.csproj", "TorrentGrease.Server/"]
COPY ["TorrentGrease.Data/TorrentGrease.Data.csproj", "TorrentGrease.Data/"]
COPY ["TorrentGrease.Shared/TorrentGrease.Shared.csproj", "TorrentGrease.Shared/"]
COPY ["TorrentGrease.TorrentClient/TorrentGrease.TorrentClient.csproj", "TorrentGrease.TorrentClient/"]
COPY ["TorrentGrease.Client/TorrentGrease.Client.csproj", "TorrentGrease.Client/"]
RUN dotnet restore "TorrentGrease.Server/TorrentGrease.Server.csproj"
COPY . .
WORKDIR "/src/TorrentGrease.Server"
RUN dotnet build "TorrentGrease.Server.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "TorrentGrease.Server.csproj" -c Release -o /app

#Temp fix for https://github.com/aspnet/Blazor/issues/376
RUN cp -rf /src/TorrentGrease.Client/dist_wwwroot/* /app/dist
#Temp fix for https://github.com/aspnet/AspNetCore/issues/9704
RUN rm -f /app/TorrentGrease.Client.blazor.config
RUN echo "/app\n/app/TorrentGrease.Client.dll" > /app/TorrentGrease.Client.blazor.config

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TorrentGrease.Server.dll"]