# 🔥 BIG TECH LEVEL: Multi-stage Docker build with security best practices
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Create non-root user for build stage
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -u 1001 -G appgroup

# Copy project files with proper ownership
COPY --chown=appuser:appgroup ["src/Presentation/TicketManagement.API/TicketManagement.API.csproj", "src/Presentation/TicketManagement.API/"]
COPY --chown=appuser:appgroup ["src/Core/TicketManagement.Application/TicketManagement.Application.csproj", "src/Core/TicketManagement.Application/"]
COPY --chown=appuser:appgroup ["src/Core/TicketManagement.Application.Contracts/TicketManagement.Application.Contracts.csproj", "src/Core/TicketManagement.Application.Contracts/"]
COPY --chown=appuser:appgroup ["src/Core/TicketManagement.Domain/TicketManagement.Domain.csproj", "src/Core/TicketManagement.Domain/"]
COPY --chown=appuser:appgroup ["src/Infrastructure/TicketManagement.Infrastructure/TicketManagement.Infrastructure.csproj", "src/Infrastructure/TicketManagement.Infrastructure/"]

# Switch to non-root user for build
USER appuser

# Restore dependencies
RUN dotnet restore "src/Presentation/TicketManagement.API/TicketManagement.API.csproj"

# Copy source code with proper ownership
COPY --chown=appuser:appgroup . .

# Build and publish
WORKDIR "/src/src/Presentation/TicketManagement.API"
RUN dotnet publish "TicketManagement.API.csproj" -c Release -o /app/publish --no-restore \
    --runtime linux-musl-x64 --self-contained false

# Runtime stage with distroless image for security
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Install curl for health checks and security updates
RUN apk add --no-cache curl && \
    apk upgrade --no-cache

# Create non-root user with specific UID/GID
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -u 1001 -G appgroup

# Create app directory with proper permissions
RUN mkdir -p /app/logs && \
    chown -R appuser:appgroup /app

WORKDIR /app

# Copy published app with proper ownership
COPY --from=build --chown=appuser:appgroup /app/publish .

# Switch to non-root user
USER appuser

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Health check with proper timeout and retries
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

# Expose port
EXPOSE 8080

# Use exec form for better signal handling
ENTRYPOINT ["dotnet", "TicketManagement.API.dll"]