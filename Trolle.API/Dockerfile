# =========================
# Base image
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /Trolle.Backend

# Copy project files first (better layer caching)
COPY ["Trolle.API/Trolle.API.csproj", "Trolle.API/"]
COPY ["Trolle.Application/Trolle.Application.csproj", "Trolle.Application/"]
COPY ["Trolle.Infrastructure/Trolle.Infrastructure.csproj", "Trolle.Infrastructure/"]
COPY ["Trolle.Domain/Trolle.Domain.csproj", "Trolle.Domain/"]

# Restore dependencies
RUN dotnet restore "Trolle.API/Trolle.API.csproj"

# Copy remaining source and build
COPY . .
WORKDIR "/Trolle.Backend/Trolle.API"
RUN dotnet build "Trolle.API.csproj" -c Release -o /app/build

# =========================
# Publish stage
# =========================
FROM build AS publish
RUN dotnet publish "Trolle.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =========================
# Final runtime image
# =========================
FROM base AS final
WORKDIR /app
EXPOSE 2207

# Install curl for container healthchecks
RUN apt-get update \
    && apt-get install -y curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published output
COPY --from=publish /app/publish .

# Start application
ENTRYPOINT ["dotnet", "Trolle.API.dll"]