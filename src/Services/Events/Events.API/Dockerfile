# ============================================
# Build Stage
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# کپی و Restore (برای بهره‌برداری از Docker Cache)
COPY src/BuildingBlocks/Common.Caching/Common.Caching.csproj src/BuildingBlocks/Common.Caching/
COPY src/BuildingBlocks/Common.Logging/Common.Logging.csproj src/BuildingBlocks/Common.Logging/
COPY src/Services/Events/Events.Domain/Events.Domain.csproj src/Services/Events/Events.Domain/
COPY src/Services/Events/Events.Application/Events.Application.csproj src/Services/Events/Events.Application/
COPY src/Services/Events/Events.Infrastructure/Events.Infrastructure.csproj src/Services/Events/Events.Infrastructure/
COPY src/Services/Events/Events.API/Events.API.csproj src/Services/Events/Events.API/

RUN dotnet restore src/Services/Events/Events.API/Events.API.csproj

# کپی کل سورس و Build
COPY . .
RUN dotnet publish src/Services/Events/Events.API/Events.API.csproj -c Release -o /app/publish --no-restore

# ============================================
# Runtime Stage
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Events.API.dll"]