# ============================================
# Build Stage
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# کپی و Restore (برای بهره‌برداری از Docker Cache)
COPY src/BuildingBlocks/Common.Logging/Common.Logging.csproj src/BuildingBlocks/Common.Logging/
COPY src/BuildingBlocks/EventBus.RabbitMQ/EventBus.RabbitMQ.csproj src/BuildingBlocks/EventBus.RabbitMQ/
COPY src/Services/Notification/Notification.API/Notification.API.csproj src/Services/Notification/Notification.API/

RUN dotnet restore src/Services/Notification/Notification.API/Notification.API.csproj

# کپی کل سورس و Build
COPY . .
RUN dotnet publish src/Services/Notification/Notification.API/Notification.API.csproj -c Release -o /app/publish --no-restore

# ============================================
# Runtime Stage
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Notification.API.dll"]