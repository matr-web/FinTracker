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

    /// <inheritdoc cref="ICurrencyService.GetExchangeRateAsync" />
    public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
    {
        // 1. Validate input - ensure that currency codes are not null, empty, or whitespace
        if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
            throw new ArgumentException("Currency codes cannot be empty.");

        // if both currencies are the same, return 1 immediately
        if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
            return 1m;

        // 2. Ensure currency codes are in uppercase as required by the API
        fromCurrency = fromCurrency.ToUpper();
        toCurrency = toCurrency.ToUpper();

        // 3. Build the API URL
        string url = $"https://api.frankfurter.app/latest?from={fromCurrency}&to={toCurrency}";

        try
        {
            // 4. Make the API call and parse the response
            var response = await _httpClient.GetFromJsonAsync<FrankfurterResponse>(url);

            // 5. Check if the response contains the expected data and return the exchange rate
            if (response?.Rates != null && response.Rates.TryGetValue(toCurrency, out decimal rate))
            {
                return rate;
            }

            // 6. If we reach here, it means the expected rate was not found in the response
            throw new Exception($"Rate for {toCurrency} not found in response.");
        }
        // 7. Handle potential exceptions that may occur during the API call
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException("Network error while fetching currency exchange rate from Frankfurter.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Unexpected error while fetching currency exchange rate.", ex);
        }
    }
}
