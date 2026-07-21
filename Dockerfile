# syntax=docker/dockerfile:1

############################
# BUILD STAGE
############################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy solution
COPY CommerceCore.sln .

# Copy project files
COPY src/CommerceCore.Api/CommerceCore.Api.csproj src/CommerceCore.Api/
COPY src/CommerceCore.Application/CommerceCore.Application.csproj src/CommerceCore.Application/
COPY src/CommerceCore.Contracts/CommerceCore.Contracts.csproj src/CommerceCore.Contracts/
COPY src/CommerceCore.Domain/CommerceCore.Domain.csproj src/CommerceCore.Domain/
COPY src/CommerceCore.Infrastructure/CommerceCore.Infrastructure.csproj src/CommerceCore.Infrastructure/

# Restore
RUN dotnet restore src/CommerceCore.Api/CommerceCore.Api.csproj

# Copy source
COPY src ./src

# Publish
RUN dotnet publish src/CommerceCore.Api/CommerceCore.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

############################
# RUNTIME STAGE
############################
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=20s --retries=3 \
CMD wget --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "CommerceCore.Api.dll"]