using System.Text.Json.Serialization;

public class BookingModel
{
   [JsonPropertyName("BookingId")]
   public int? BookingId { get; set; }

  
   [JsonPropertyName("RestaurantName")]
   public string? RestaurantName { get; set; }

   [JsonPropertyName("Count")]
   public int? Count { get; set; }

   [JsonPropertyName("BookingDate")]
   public string? BookingDate { get; set; }

   [JsonPropertyName("CustomerName")]
   public string? CustomerName { get; set; }

   [JsonPropertyName("CustomerEmail")]
   public string? CustomerEmail { get; set; }

   [JsonPropertyName("CustomerPhone")]
   public string? CustomerPhone { get; set; }

   [JsonPropertyName("Status")]
   public Status? Status { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status
{
   Booked,
   NotBooked,
   Cancelled
}

