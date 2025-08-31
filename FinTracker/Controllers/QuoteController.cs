using FinTracker.WebAPI.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FinTracker.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QuoteController : ControllerBase
{
    [HttpGet("GetQuote")]
    public async Task<QuoteEntity?> GetQuoteAsync(string symbol)
    {
        string apiKey = "d2c745pr01qvh3veatf0d2c745pr01qvh3veatfg"; // Finhub API Key.
        string url = $"https://finnhub.io/api/v1/quote?symbol={symbol}&token={apiKey}";

        using var client = new HttpClient();

        var quote = await client.GetFromJsonAsync<QuoteEntity>(url);

        return quote;
    }
}
