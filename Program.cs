#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0060

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;
// Add this using directive

// Create kernel
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4o",
    endpoint: "https://openai-levelup.openai.azure.com/",
    apiKey: "c6da773c07bc45de992ecd85c60f4162",
    modelId: "gpt-4o" // optional
);
var kernel = builder.Build();

List<BookingModel> bookings = new(){
    new BookingModel { BookingId = 1, RestaurantName = "The Ivy Clifton Brasserie", Count = 2, BookingDate = DateTime.Now.ToString(), CustomerName = "John Doe", CustomerEmail = "john.doe@gmail.com" , CustomerPhone = "1234567890", Status = Status.Booked },
    new BookingModel { BookingId = 2, RestaurantName = "The Ox", Count = 2, BookingDate = DateTime.Now.ToString(), CustomerName = "Jane Doe",CustomerEmail = "john.doe@gmail.com" , CustomerPhone = "1234567890", Status = Status.Booked},
    new BookingModel { BookingId = 3, RestaurantName = "The Lido", Count = 2, BookingDate = DateTime.Now.ToString(), CustomerName = "John Smith", CustomerEmail = "john.smith@gmail.com" , CustomerPhone = "1234567890", Status = Status.Booked},
};

kernel.Plugins.AddFromObject(new BookingsPlugin(bookings));

//import plugins
kernel.ImportPluginFromType<SearchPlugin>();

// Suppress the warning about ConversationSummaryPlugin
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
kernel.ImportPluginFromType<ConversationSummaryPlugin>();
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var prompts = kernel.ImportPluginFromPromptDirectory("prompts");

ChatHistory history = [];
string input = @"I'm planning an anniversary date with my spouse. We like non vegetarian food and are looking for a romantic restaurant in Bristol, UK. We are looking for a place that serves alcohol and has a good view. Please suggest top 5 ";

// var plugins = kernel.CreatePluginFromPromptDirectory("get_intent");
// string input = "Suggest me popular restaurants in Bristol UK with cost for 2 people? ";
// //string input1= "who is the prime minister of UK";


var result = await ExecuteWithFunctions();
history.AddUserMessage(input);
if (result != null)
{
    history.AddAssistantMessage(result);
}


#region functions

// https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/adding-native-plugins?pivots=programming-language-csharp
// Write a native function that calls a REST API (e.g. Bing search) to automatically retrieve the day and time of the next [your favorite team 
// and sport] game in order to be integrated in the email.
async Task<string?> SearchWebForRestaurants(string location, string cuisineType = "non vegetarian", string ratings = "good", string numberofpeople = "100")
{
    var web_search_result = await kernel.InvokeAsync<string>("SearchPlugin", 
    "SearchRestaurant",
    new() {
        { "location", location }, { "cuisineType", cuisineType }, { "ratings", ratings }, { "numberofpeople", numberofpeople }
    });

    Console.WriteLine(web_search_result);
    return web_search_result;
}

async Task<string?> BookRestaurant(Kernel kernel, KernelArguments args)
{
        string restaurantName = args["RestaurantName"].ToString();
        string bookingDate = args["BookingDate"].ToString();
        int count = (int)args["Count"];
        string personName = args["CustomerName"].ToString();
        string email = args["CustomerEmail"].ToString();
        string phoneNumber = args["CustomerPhone"].ToString();

    var booking = await kernel.InvokeAsync<string>("BookingsPlugin", "AddBooking", 
    new() {
            { "restaurantName", restaurantName }, { "DateTime", bookingDate }, { "count", count }, { "personName", personName }, { "email", email }, { "phoneno", phoneNumber }
        }
    );
    return booking;
}


async Task<string?> GetReservationEmail(Kernel kernel, KernelArguments args)
{ 
    Console.WriteLine("Email 1 step");
      // var prompts = kernel.ImportPluginFromPromptDirectory("prompts");
    var emailReservation = await kernel.InvokeAsync<string>(prompts["emails"], args);
    Console.WriteLine("Email 2 step");
    Console.WriteLine(emailReservation);
    return emailReservation;
}

async Task RunAutoFunctionCalling(){
    OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
    IChatCompletionService chat = kernel.GetRequiredService<IChatCompletionService>();
    ChatHistory chatHistory = new();
     int iteration = 0;
      while (true)
        {
            Console.Write("Question (Type \"quit\" to leave): ");

            //string question = System.Console.ReadLine() ?? string.Empty;

            // Comment out this line and uncomment the one above to run in a console chat loop.
            string question = iteration == 0 ? "What do you want to do? (e.g. 'Suggest me popular restaurants in Bristol UK with cost for 2 people?')" : "quit";

            if (question == "quit")
            {
                break;
            }

            chatHistory.AddUserMessage(question);
            StringBuilder sb = new();
            await foreach (var update in chat.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel))
            {
                if (update.Content is not null)
                {
                    Console.Write(update.Content);
                    sb.Append(update.Content);
                }
            }
            chatHistory.AddAssistantMessage(sb.ToString());
            Console.WriteLine();
            iteration++;
        }
}

async Task<string?> ExecuteWithFunctions()
{
    var result = string.Empty;
    Console.WriteLine("What do you want to do? (e.g. 'Suggest me popular restaurants in Bristol UK with cost for 2 people?')");
    var input = Console.ReadLine();

    string chatPrompt = """
            <message role="system"> You are a Restaurant Recommendation Bot. Your role is to assist users in finding the best restaurants based on their preferences for food quality, ambiance, and music. You have an extensive knowledge of various dining options, including cuisine types, atmospheres, and entertainment offerings. When providing recommendations, consider the user's location, cusine type, ratings, number of people rated and the type of dining experience they are seeking. Be friendly, informative, and concise in your suggestions, offering a brief description of each restaurant and why it might be a perfect fit for the user. Identify the user's intent. Return one of the following values:
            SuggestRestaurants - If the user wants restuarants recommendations
            Unknown - If the user's intent matches none of the above.</message>

            for example:
            <message role="user">Can you suggest me top 5 indian restaurants in Birmingham UK with a rough estimate for 2 people?</message>
            <message role="assistant">SuggestRestaurants</message>

            <message role="user">I would like to book a table at The Ivy Clifton Brasserie for two people on 31st December at 7 PM.</message>
            <message role="assistant">BookRestaurant</message>

            <message role="user">Learn about advanced physics</message>
            <message role="assistant">Unknown</message>
            """;


    OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

    var result1 = await kernel.InvokePromptAsync(chatPrompt, new(settings));
    // var intent = await kernel.InvokeAsync<string>(
    //     prompts["get_intent"], 
    //     new() {{ "input",  input }}
    // );
    Console.WriteLine("Auto Kernel function calling");
/*
    switch (intent) {
        case "SuggestRestaurants":
                Console.WriteLine("Which location are you looking for?");
                var location = Console.ReadLine();
                Console.WriteLine("What cusine are you looking for?");
                var cusine = Console.ReadLine();
                Console.WriteLine("What ratings are you looking for?");
                var ratings = Console.ReadLine();
                Console.WriteLine("how many people should have rated it?");
                var people = Console.ReadLine();
                result = await SearchWebForRestaurants(location, cusine, ratings, people);
                Console.WriteLine(result);
                Console.WriteLine("Which restuarant you would want to choose?");
                var restaurantName = Console.ReadLine();
                Console.WriteLine("What date and time would you like to book? (e.g. '2022-12-31 23:59')");
                var bookingDate = Console.ReadLine();
                Console.WriteLine("How many people?");
                var countInput = Console.ReadLine();
                var count = countInput != null ? int.Parse(countInput) : 0;
                Console.WriteLine("What is your name?");
                var personName = Console.ReadLine();
                Console.WriteLine("What is your email?");
                var emailid = Console.ReadLine();
                Console.WriteLine("What is your phone number?");
                var phoneno = Console.ReadLine();
                var args = new KernelArguments{
                    { "RestaurantName", restaurantName },
                    { "BookingDate", bookingDate },
                    { "Count", count },
                    { "CustomerName", personName },
                    { "CustomerEmail", emailid },
                    { "CustomerPhone", phoneno },
                };

                var bookingresult = await BookRestaurant(kernel, args);

                var reservationEmail =await GetReservationEmail(kernel, args);
                Console.WriteLine(reservationEmail);              
            break;
        default:
            Console.WriteLine("Sorry, I can't help with that.");
            break;
    }
*/
    return "Booking done";
    
}
#endregion