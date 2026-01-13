# FinTrack Host - Multi-stage Dockerfile
# Build: docker build -t fintrack -f src/FinTrack.Host/Dockerfile .

# Stage 1: Build .NET application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY FinTrack.sln .
COPY Directory.Build.props .
COPY src/FinTrack.Host/FinTrack.Host.csproj src/FinTrack.Host/
COPY src/FinTrack.Core/FinTrack.Core.csproj src/FinTrack.Core/
COPY src/FinTrack.Infrastructure/FinTrack.Infrastructure.csproj src/FinTrack.Infrastructure/

# Restore dependencies
RUN dotnet restore src/FinTrack.Host/FinTrack.Host.csproj

# Copy source code
COPY src/FinTrack.Host/ src/FinTrack.Host/
COPY src/FinTrack.Core/ src/FinTrack.Core/
COPY src/FinTrack.Infrastructure/ src/FinTrack.Infrastructure/

# Build application
RUN dotnet publish src/FinTrack.Host/FinTrack.Host.csproj -c Release -o /app/publish --no-restore

# Stage 2: Build React application
FROM node:22-alpine AS frontend
WORKDIR /app

# Copy package files
COPY src/FinTrack.Host/ClientApp/package*.json ./

# Install dependencies
RUN npm ci

# Copy source and build
COPY src/FinTrack.Host/ClientApp/ ./
RUN npm run build

# Stage 3: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd -r fintrack && useradd -r -g fintrack fintrack

# Create directories for app data
RUN mkdir -p /app/data /app/temp && chown -R fintrack:fintrack /app

# Copy published application
COPY --from=build /app/publish .

# Copy React build output to wwwroot
COPY --from=frontend /app/dist ./wwwroot

# Switch to non-root user
USER fintrack

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "FinTrack.Host.dll"]
