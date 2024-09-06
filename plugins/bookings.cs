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
    [Description("List reservations booking at a restaurant.")]
    [return: Description("Get all the bookings made by the user.")]
    public async Task<List<BookingModel>> GetBookings()
    {
         Console.WriteLine("GetBookings method was called");
        return bookings;
    }

    [KernelFunction("AddBooking")]
    [Description("Books a reservation at a restaurant.")]
    public async Task AddBooking(
        [Description("Name of the restuarant")] string restaurantName, 
        [Description("The time in UTC")] string DateTime, 
        [Description("Number of people")] int count, 
        [Description("Customer Name")] string personName, 
        [Description("Customer email")] string email, 
        [Description("Customer Phone number")] string phoneno)
    {
        Console.WriteLine("BookRestaurant method was called");
        Console.WriteLine($"System > Do you want to book a table at {restaurantName} on {DateTime} for {count} people?");
        Console.WriteLine("System > Please confirm by typing 'yes' or 'no'.");
        Console.Write("User > ");
        var response = Console.ReadLine()?.Trim();
        
        if (string.Equals(response, "yes", StringComparison.OrdinalIgnoreCase))
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
       else {
        Console.WriteLine("Booking aborted by the user");
       }
    }

    [KernelFunction("CancelBooking")]
    [Description("Book the restaurant with the details provided by the user: Name, Date time, Number of people, your name, email address and phone number.")]
    [return: Description("Cancels a reservation at a restaurant.")]
    public async Task CancelBooking(
        [Description("The booking ID to cancel")] int BookingId 
    )
    {
        Console.WriteLine("CancelBooking method was called");
        var booking = bookings.Find(b => b.BookingId == BookingId);
        if (booking != null)
        {
            booking.Status = Status.Cancelled;
            Console.WriteLine($"Booking {BookingId} cancelled successfully.");
        }
        else
        {
            Console.WriteLine($"Booking {BookingId} not found.");
        }
    }  
}
