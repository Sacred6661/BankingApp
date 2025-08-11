# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["scr/AccountService/AccountService.csproj", "scr/AccountService/"]
COPY ["scr/Shared/Shared.csproj", "scr/Shared/"]
RUN dotnet restore "./scr/AccountService/AccountService.csproj"
COPY . .
WORKDIR "/src/scr/AccountService"
RUN dotnet build "./AccountService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AccountService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
ADD https://github.com/jwilder/dockerize/releases/download/v0.7.0/dockerize-linux-amd64-v0.7.0.tar.gz /tmp/dockerize.tar.gz
RUN tar -C /usr/local/bin -xzvf /tmp/dockerize.tar.gz && rm /tmp/dockerize.tar.gz
USER app

ENTRYPOINT ["dockerize", "-wait", "tcp://postgres-accounts:5432", "-wait", "tcp://authserver:8080", "-timeout", "60s", "dotnet", "AccountService.dll"]