using Eventus.AppServices.Data.Domain.Bookings;
using Eventus.AppServices.Data.Enums.Bookings;
using Eventus.AppServices.Exceptions;

namespace Eventus.Tests.Unit.Bookings;

public class BookingDomainTests
{
    private static void AssertRejectedBooking(Booking previous, Booking? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(previous.Id, actual.Id);
        Assert.Equal(previous.EventId, actual.EventId);
        Assert.Equal(BookingStatus.Rejected, actual.Status);
        Assert.Equal(previous.CreatedAt, actual.CreatedAt);
        Assert.NotNull(actual.ProcessedAt);
    }

    public static TheoryData<string, Guid, Guid, string> IncorrectRequests => new()
    {
        { "EmptyBookingId", Guid.Empty, TestDataFactory.EventId, nameof(Booking.Id) },
        { "EmptyEventId", TestDataFactory.BookingId, Guid.Empty, nameof(Booking.EventId) }
    };

    #region NegativeCases

    [Fact]
    public void ConfirmBooking_ConfirmedBooking_ThrowsBookingProcessingException()
    {
        // Arrange
        var booking = TestDataFactory.PendingBooking;
        var confirmedBooking = booking.Confirm();

        // Act & Assert
        Assert.Throws<BookingProcessingException>(confirmedBooking.Confirm);
    }

    [Fact]
    public void ConfirmBooking_RejectedBooking_ThrowsBookingProcessingException()
    {
        // Arrange
        var booking = TestDataFactory.PendingBooking;
        var rejectedBooking = booking.Reject();

        // Act & Assert
        Assert.Throws<BookingProcessingException>(rejectedBooking.Confirm);
    }

    [Fact]
    public void RejectBooking_ConfirmedBooking_ThrowsBookingProcessingException()
    {
        // Arrange
        var booking = TestDataFactory.PendingBooking;
        var confirmedBooking = booking.Confirm();

        // Act & Assert
        Assert.Throws<BookingProcessingException>(confirmedBooking.Reject);
    }

    [Fact]
    public void RejectBooking_RejectedBooking_ThrowsBookingProcessingException()
    {
        // Arrange
        var booking = TestDataFactory.PendingBooking;
        var rejectedBooking = booking.Reject();

        // Act & Assert
        Assert.Throws<BookingProcessingException>(rejectedBooking.Reject);
    }

    [Fact]
    public void TryRejectBooking_ConfirmedBooking_ReturnsFalseAndNull()
    {
        // Arrange
        var booking = TestDataFactory.PendingBooking;
        var confirmedBooking = booking.Confirm();

        // Act
        var isRejected = confirmedBooking.TryReject(out var actual);

        // Assert
        Assert.False(isRejected);
        Assert.Null(actual);
    }

    [Fact]
    public void TryRejectBooking_RejectedBooking_ReturnsFalseAndNull()
    {
        // Arrange
        var booking = TestDataFactory.PendingBooking;
        var rejectedBooking = booking.Reject();

        // Act
        var isRejected = rejectedBooking.TryReject(out var actual);

        // Assert
        Assert.False(isRejected);
        Assert.Null(actual);
    }

    [Theory]
    [MemberData(nameof(IncorrectRequests))]
    public void CreateBooking_IncorrectRequests_ThrowsDomainValidationException(
        string _, Guid bookingId, Guid eventId, string field)
    {
        // Arrange
        // Act
        var exception = Assert.Throws<DomainValidationException>(() => new Booking(bookingId, eventId));

        // Assert
        Assert.Equal(field, exception.ProblemFieldName);
    }

    #endregion

    #region PositiveCases

    [Fact]
    public void ConfirmBooking_PendingBooking_ReturnsNewConfirmedBooking()
    {
        // Arrange
        var booking = TestDataFactory.PendingBooking;
        var oldStatus = booking.Status;

        // Act
        var actual = booking.Confirm();

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(booking.Id, actual.Id);
        Assert.Equal(booking.EventId, actual.EventId);
        Assert.Equal(BookingStatus.Confirmed, actual.Status);
        Assert.Equal(booking.CreatedAt, actual.CreatedAt);
        Assert.NotNull(actual.ProcessedAt);
        Assert.True(actual.ProcessedAt >= actual.CreatedAt);
        Assert.Equal(oldStatus, booking.Status);
    }

    [Fact]
    public void RejectBooking_PendingBooking_ReturnsNewRejectedBooking()
    {
        // Arrange
        var booking = TestDataFactory.PendingBooking;
        var oldStatus = booking.Status;

        // Act
        var rejectedBooking = booking.Reject();

        // Assert
        AssertRejectedBooking(booking, rejectedBooking);
        Assert.Equal(oldStatus, booking.Status);
    }

    [Fact]
    public void TryRejectBooking_PendingBooking_ReturnsTrueAndRejectedBooking()
    {
        // Arrange
        var booking = TestDataFactory.PendingBooking;
        var oldStatus = booking.Status;

        // Act
        var isRejected = booking.TryReject(out var actual);

        // Assert
        Assert.True(isRejected);
        AssertRejectedBooking(booking, actual);
        Assert.Equal(oldStatus, booking.Status);
    }

    [Fact]
    public void CreateBooking_CorrectRequests_ReturnsPendingBooking()
    {
        // Arrange
        var bookingId = TestDataFactory.BookingId;
        var eventId = TestDataFactory.EventId;

        // Act
        var actual = new Booking(bookingId, eventId);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(bookingId, actual.Id);
        Assert.Equal(eventId, actual.EventId);
        Assert.Equal(BookingStatus.Pending, actual.Status);
        Assert.Null(actual.ProcessedAt);
    }

    #endregion
}