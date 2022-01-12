FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
RUN apk add curl

WORKDIR /app

EXPOSE 5656
EXPOSE 5657
#Unset default env var
ENV ASPNETCORE_URLS=

HEALTHCHECK --interval=30s --timeout=10s --retries=3 CMD curl --fail http://localhost:5656/health || exit 1

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
ARG VERSION=0.1.0-devbuild
WORKDIR /src
# Copy the main source project files
COPY */*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done

RUN dotnet restore "TorrentGrease.Server/TorrentGrease.Server.csproj"
COPY . .
WORKDIR "/src/TorrentGrease.Server"
RUN dotnet build "TorrentGrease.Server.csproj" -c Release -o /app/build -p:Version=${VERSION} /p:warnaserror=true /p:TreatWarningsAsErrors=true --no-restore

FROM build AS publish
ARG VERSION=0.1.0-devbuild
RUN dotnet publish "TorrentGrease.Server.csproj" -c Release -o /app/publish -p:Version=${VERSION} --no-restore

FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TorrentGrease.Server.dll"]