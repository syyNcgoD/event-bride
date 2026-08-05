using System.Net.Http.Json;
using Booking.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Booking.Infrastructure.Services;

/// <summary>
/// دسترسی به Events Service برای موجودی بلیط
/// </summary>
public class TicketInventoryService : ITicketInventoryService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TicketInventoryService> _logger;

    public TicketInventoryService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TicketInventoryService> logger)
    {
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(
            configuration["Services:EventsApi"] ?? "http://localhost:5002");
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<TicketAvailability?> GetTicketAvailabilityAsync(
        int ticketTypeId, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: در آینده با gRPC جایگزین می‌شود
            var response = await _httpClient.GetFromJsonAsync<EventTicketDto>(
                $"api/Events/tickets/{ticketTypeId}", cancellationToken);

            if (response is null)
            {
                return null;
            }

            return new TicketAvailability(
                response.Id,
                response.EventId,
                response.EventTitle,
                response.Name,
                response.Price,
                response.Quantity - response.SoldCount,
                response.MaxPerOrder);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get ticket availability for {TicketTypeId}", ticketTypeId);
            return null;
        }
    }

    public async Task<bool> ReserveTicketsAsync(
        int ticketTypeId, int eventId, int quantity, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/Events/tickets/{ticketTypeId}/reserve",
                new { EventId = eventId, Quantity = quantity },
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reserve tickets for {TicketTypeId}", ticketTypeId);
            return false;
        }
    }

    public async Task<bool> ReleaseTicketsAsync(
        int ticketTypeId, int quantity, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/Events/tickets/{ticketTypeId}/release",
                new { Quantity = quantity },
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to release tickets for {TicketTypeId}", ticketTypeId);
            return false;
        }
    }

    private sealed class EventTicketDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int SoldCount { get; set; }
        public int MaxPerOrder { get; set; }
    }
}