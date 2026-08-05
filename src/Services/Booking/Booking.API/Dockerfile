# ============================================
# Build Stage
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# کپی و Restore (برای بهره‌برداری از Docker Cache)
COPY src/BuildingBlocks/Common.Caching/Common.Caching.csproj src/BuildingBlocks/Common.Caching/
COPY src/BuildingBlocks/Common.Logging/Common.Logging.csproj src/BuildingBlocks/Common.Logging/
COPY src/BuildingBlocks/EventBus.RabbitMQ/EventBus.RabbitMQ.csproj src/BuildingBlocks/EventBus.RabbitMQ/
COPY src/Services/Booking/Booking.Domain/Booking.Domain.csproj src/Services/Booking/Booking.Domain/
COPY src/Services/Booking/Booking.Application/Booking.Application.csproj src/Services/Booking/Booking.Application/
COPY src/Services/Booking/Booking.Infrastructure/Booking.Infrastructure.csproj src/Services/Booking/Booking.Infrastructure/
COPY src/Services/Booking/Booking.API/Booking.API.csproj src/Services/Booking/Booking.API/

RUN dotnet restore src/Services/Booking/Booking.API/Booking.API.csproj

# کپی کل سورس و Build
COPY . .
RUN dotnet publish src/Services/Booking/Booking.API/Booking.API.csproj -c Release -o /app/publish --no-restore

# ============================================
# Runtime Stage
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Booking.API.dll"]