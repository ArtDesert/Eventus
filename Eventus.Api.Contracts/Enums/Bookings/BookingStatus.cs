using System.Text.Json.Serialization;

namespace Eventus.Api.Contracts.Enums.Bookings;

[JsonConverter(typeof(JsonStringEnumConverter<BookingStatus>))]
public enum BookingStatus
{
    Unknown = 0,
    Pending = 1,
    Confirmed = 2,
    Rejected = 3,
}