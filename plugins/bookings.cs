#pragma warning disable SKEXP0050

using System.ComponentModel;
using Microsoft.SemanticKernel;

public class BookingsPlugin{

    private List<BookingModel> bookings;

    public BookingsPlugin(List<BookingModel> bookings)
    {
    this.bookings = bookings;
    }

    [KernelFunction("GetBookings")]
    [Description("Get all the bookings made by the user.")]
    [return: Description("Get all the bookings made by the user.")]
    public async Task<List<BookingModel>> GetBookings()
    {
        return bookings;
    }

    [KernelFunction("AddBooking")]
    [Description("Book the restaurant with the details provided by the user: Name, Date time, Number of people, your name, email address and phone number.")]
    [return: Description("Book the restaurant with the details provided by the user: Name, Date time, Number of people, your name, email address and phone number.")]
    public async Task AddBooking([Description("restuarant")] string restaurantName, string DateTime, int count, string personName, string email, string phoneno)
    {
        var newbooking = new BookingModel
        {
            BookingId =  new Random().Next(1, 1000),
            RestaurantName = restaurantName,
            BookingDate = DateTime,
            Count = count,
            CustomerName = personName,
            CustomerEmail = email,
            CustomerPhone = phoneno,
            Status = Status.Booked
        };
        this.bookings.Add(newbooking);
        Console.WriteLine($"Booking Details: Booking Id {newbooking.BookingId}, Booking Date {newbooking.BookingDate}, Booking Count: {newbooking.Count}, Customer: {newbooking.CustomerName}, Email: {newbooking.CustomerEmail}, Phone: {newbooking.CustomerPhone} added successfully.");
       }

     [KernelFunction("EditBooking")]
    [Description("Book the restaurant with the details provided by the user: Name, Date time, Number of people, your name, email address and phone number.")]
    [return: Description("Book the restaurant with the details provided by the user: Name, Date time, Number of people, your name, email address and phone number.")]
    public async Task EditBooking([Description("restuarant")] int bookingId, string customerName, string bookingDate, int count, BookingModel bookingStatus)
    {
        var booking = bookings.Find(b => b.BookingId == bookingId);
        if (booking != null)
        {
            booking.CustomerName = customerName;
            booking.BookingDate = bookingDate;
            booking.Count = count;
            booking.Status = bookingStatus.Status;
            Console.WriteLine($"Booking {bookingId} updated successfully.");
        }
        else
        {
            Console.WriteLine($"Booking {bookingId} not found.");
        }
    }

     [KernelFunction("DeleteBooking")]
    [Description("Book the restaurant with the details provided by the user: Name, Date time, Number of people, your name, email address and phone number.")]
    [return: Description("Book the restaurant with the details provided by the user: Name, Date time, Number of people, your name, email address and phone number.")]
    public async Task DeleteBooking([Description("restuarant")] int bookingId)
    {
        var booking = bookings.Find(b => b.BookingId == bookingId);
        if (booking != null)
        {
            bookings.Remove(booking);
            Console.WriteLine($"Booking {bookingId} deleted successfully.");
        }
        else
        {
            Console.WriteLine( $"Booking {bookingId} not found.");
        }
    }
}
