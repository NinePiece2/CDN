#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

USER root  # Switch to root user for package installation
RUN apt-get update && apt-get install -y curl && apt-get clean && rm -rf /var/lib/apt/lists/*
USER app  # Switch back to the non-root user

WORKDIR /app
EXPOSE 8080
EXPOSE 8443

# Add passive FTP port range
EXPOSE 21
EXPOSE 20
EXPOSE 30000-31000


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CDN/CDN.csproj", "CDN/"]
RUN dotnet restore "./CDN/CDN.csproj"
COPY . .
WORKDIR "/src/CDN"
RUN dotnet build "./CDN.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CDN.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CDN.dll"]