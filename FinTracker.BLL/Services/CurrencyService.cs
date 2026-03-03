using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.Response;
using System.Net.Http.Json;

namespace FinTracker.BLL.Services;

public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _httpClient;

    public CurrencyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal> GetExchangeRateFrankfurterAsync(string fromCurrency, string toCurrency)
    {
        // URL for Frankfurter: https://api.frankfurter.app/latest?from=USD&to=PLN
        string url = $"https://api.frankfurter.app/latest?from={fromCurrency}&to={toCurrency}";

        try
        {
            var response = await _httpClient.GetFromJsonAsync<FrankfurterResponse>(url);

            if (response != null && response.Rates.ContainsKey(toCurrency))
            {
                return response.Rates[toCurrency];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }

        return 1m; // Return 1 if there's an error or if the rate is not found.
    }
}
