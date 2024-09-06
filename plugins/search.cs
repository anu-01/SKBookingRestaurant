#pragma warning disable SKEXP0050

using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

/// <summary>
/// A plugin that searches the web to find the best resturants.
/// https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/using-data-retrieval-functions-for-rag
/// </summary>
public class SearchPlugin
{
    [KernelFunction("SearchRestaurant")]
    [Description("Search for the best restaurants in a location based on cuisine type, ratings, number of people who rated.")]
    [return: Description("The best restaurants in a location based on cuisine type, ratings, number of people who rated.")]
    public async Task<string> WebSearch(
        [Description("Location ")] string location, 
        [Description("Type of the cuisine ")] string cuisineType, 
        [Description("Ratings")] string ratings, 
        [Description("Number of ratings ")] string numberofpeople)
    {
        Console.WriteLine("SearchRestaurant method was called");
        // Write a native function that calls a REST API (e.g. Bing search) to automatically retrieve the best restaurants in the city.
        var kernel = Kernel.CreateBuilder().Build();
        
        var bingConnector = new BingConnector("f2e46ea03acd4270aa9f9e59a10e1b3a");
        kernel.ImportPluginFromObject(new WebSearchEnginePlugin(bingConnector), "bing");

        var function = kernel.Plugins["bing"]["search"];
        var bingResult = await kernel.InvokeAsync(function, new() { ["query"] = "Suggest me good restuarants in "+ location + " based on Cuisine Type" + cuisineType + " with ratings " + ratings + " and number of people who rated " + numberofpeople });
        return bingResult.ToString();
    }
}