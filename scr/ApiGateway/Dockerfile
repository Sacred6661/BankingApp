# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ENVIRONMENT variable to use it (for example to use appsettings.Docker.json)
ENV ASPNETCORE_ENVIRONMENT=Docker

# use root touser to allow to make rules for wait-for-it.sh
USER root

# Copy script wait-for-it.sh and make it executable 
COPY scripts/wait-for-it.sh /wait-for-it.sh
RUN chmod +x /wait-for-it.sh

# Change back to app user
USER app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["scr/ApiGateway/ApiGateway.csproj", "scr/ApiGateway/"]
RUN dotnet restore "./scr/ApiGateway/ApiGateway.csproj"
COPY . .
WORKDIR "/src/scr/ApiGateway"
RUN dotnet build "./ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build


# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

# run script to wait authserver, after that start service
ENTRYPOINT ["/wait-for-it.sh", "authserver:8080", "--", "dotnet", "ApiGateway.dll"]