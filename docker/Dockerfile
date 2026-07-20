# syntax=docker/dockerfile:1

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first and restore, so Docker layer caching skips restore
# on rebuilds where only source code (not dependencies) changed.
COPY src/CommerceCore.Shared/CommerceCore.Shared.csproj src/CommerceCore.Shared/
COPY src/CommerceCore.Domain/CommerceCore.Domain.csproj src/CommerceCore.Domain/
COPY src/CommerceCore.Contracts/CommerceCore.Contracts.csproj src/CommerceCore.Contracts/
COPY src/CommerceCore.Application/CommerceCore.Application.csproj src/CommerceCore.Application/
COPY src/CommerceCore.Infrastructure/CommerceCore.Infrastructure.csproj src/CommerceCore.Infrastructure/
COPY src/CommerceCore.Api/CommerceCore.Api.csproj src/CommerceCore.Api/
RUN dotnet restore src/CommerceCore.Api/CommerceCore.Api.csproj

# Now copy everything else and publish.
COPY src/ src/
RUN dotnet publish src/CommerceCore.Api/CommerceCore.Api.csproj -c Release -o /app/publish --no-restore

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Run as a non-root user.
RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CommerceCore.Api.dll"]
