namespace Eventus.WebApi.Controllers.WebRoutes;

public static class BookingsControllerWebRoutes
{
    public const string CreateBooking = "api/events/{eventId:guid}/book";

    public const string GetBookingById = "api/bookings/{id:guid}";

    public static string CreateBookingUrl(Guid eventId) => $"api/events/{eventId}/book";

    public static string GetBookingByIdUrl(Guid id) => $"api/bookings/{id}";
}