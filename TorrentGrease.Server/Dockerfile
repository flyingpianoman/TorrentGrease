FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.0-alpine3.10 AS base
WORKDIR /app

EXPOSE 5656
EXPOSE 5657
#Unset default env var
ENV ASPNETCORE_URLS=

HEALTHCHECK --interval=30s --timeout=10s --retries=3 CMD curl --fail http://localhost:5656/health || exit 1

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.100-alpine3.10 AS build
ARG VERSION=0.1.0-devbuild
WORKDIR /src
COPY ["TorrentGrease.Server/TorrentGrease.Server.csproj", "TorrentGrease.Server/"]
COPY ["TorrentGrease.GrpcClient/TorrentGrease.GrpcClient.csproj", "TorrentGrease.GrpcClient/"]
COPY ["TorrentGrease.Data/TorrentGrease.Data.csproj", "TorrentGrease.Data/"]
COPY ["TorrentGrease.Shared/TorrentGrease.Shared.csproj", "TorrentGrease.Shared/"]
COPY ["TorrentGrease.TorrentClient/TorrentGrease.TorrentClient.csproj", "TorrentGrease.TorrentClient/"]
COPY ["TorrentGrease.Client/TorrentGrease.Client.csproj", "TorrentGrease.Client/"]
RUN dotnet restore "TorrentGrease.Server/TorrentGrease.Server.csproj"
COPY . .
WORKDIR "/src/TorrentGrease.Server"
RUN dotnet build "TorrentGrease.Server.csproj" -c Release -o /app/build -p:Version=${VERSION} /p:warnaserror=true /p:TreatWarningsAsErrors=true

FROM build AS publish
ARG VERSION=0.1.0-devbuild
RUN dotnet publish "TorrentGrease.Server.csproj" -c Release -o /app/publish -p:Version=${VERSION}

FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TorrentGrease.Server.dll"]