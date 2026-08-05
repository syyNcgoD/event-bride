# ============================================
# Build Stage
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Gateway/ApiGateway/ApiGateway.csproj src/Gateway/ApiGateway/

RUN dotnet restore src/Gateway/ApiGateway/ApiGateway.csproj

COPY . .
RUN dotnet publish src/Gateway/ApiGateway/ApiGateway.csproj -c Release -o /app/publish --no-restore

# ============================================
# Runtime Stage
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "ApiGateway.dll"]