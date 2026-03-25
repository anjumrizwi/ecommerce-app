# ─── Stage 1: Restore + Build ────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching.
COPY EcommerceApp.sln ./
COPY Ecommerce.API/Ecommerce.API.csproj               Ecommerce.API/
COPY Ecommerce.Application/Ecommerce.Application.csproj Ecommerce.Application/
COPY Ecommerce.Application.Tests/Ecommerce.Application.Tests.csproj Ecommerce.Application.Tests/
COPY Ecommerce.Domain/Ecommerce.Domain.csproj         Ecommerce.Domain/
COPY Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj Ecommerce.Infrastructure/

RUN dotnet restore

# Copy remaining source.
COPY . .

# ─── Stage 2: Test ───────────────────────────────────────────────────────────
FROM build AS test
RUN dotnet test Ecommerce.Application.Tests/Ecommerce.Application.Tests.csproj \
    --no-restore \
    --configuration Release \
    --logger "console;verbosity=normal"

# ─── Stage 3: Publish ────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish Ecommerce.API/Ecommerce.API.csproj \
    --no-restore \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

# ─── Stage 4: Runtime ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Run as non-root to follow security best practice.
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=publish /app/publish .

# Production environment is set here; override via Azure App Service App Settings.
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Ecommerce.API.dll"]
