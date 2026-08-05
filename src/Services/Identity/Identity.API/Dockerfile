# ============================================
# Build Stage
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# کپی و Restore (برای بهره‌برداری از Docker Cache)
COPY src/BuildingBlocks/Common.Logging/Common.Logging.csproj src/BuildingBlocks/Common.Logging/
COPY src/Services/Identity/Identity.Domain/Identity.Domain.csproj src/Services/Identity/Identity.Domain/
COPY src/Services/Identity/Identity.Application/Identity.Application.csproj src/Services/Identity/Identity.Application/
COPY src/Services/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj src/Services/Identity/Identity.Infrastructure/
COPY src/Services/Identity/Identity.API/Identity.API.csproj src/Services/Identity/Identity.API/

RUN dotnet restore src/Services/Identity/Identity.API/Identity.API.csproj

# کپی کل سورس و Build
COPY . .
RUN dotnet publish src/Services/Identity/Identity.API/Identity.API.csproj -c Release -o /app/publish --no-restore

# ============================================
# Runtime Stage
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Identity.API.dll"]