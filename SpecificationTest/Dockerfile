FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base

WORKDIR /test
#Unset default env var
ENV ASPNETCORE_URLS=

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
ARG VERSION=0.1.0-devbuild
WORKDIR /src
# Copy the main source project files
COPY */*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done

RUN dotnet restore "SpecificationTest/SpecificationTest.csproj"
COPY . .
WORKDIR "/src/SpecificationTest"
RUN dotnet build "SpecificationTest.csproj" -c Debug /p:warnaserror=true /p:TreatWarningsAsErrors=true --no-restore

ENTRYPOINT ["dotnet", "test", "/src/SpecificationTest", "--no-restore"]