using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.Response;
using YahooFinanceApi;

namespace FinTracker.BLL.Services;

public class StockService : IStockService
{
    private readonly HttpClient _httpClient;

    public StockService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<StockDataResponse?> GetStockDataAsync(string symbol)
    {
        try
        {
            // Zapytanie do Yahoo Finance o konkretne pola
            var query = await Yahoo.Symbols(symbol)
                                   .Fields(Field.RegularMarketPrice,
                                           Field.Currency,
                                           Field.LongName,
                                           Field.RegularMarketTime)
                                   .QueryAsync();

            if (!query.ContainsKey(symbol))
            {
                return null; // Lub rzuć wyjątek, jeśli symbol jest błędny
            }

            var ticker = query[symbol];

            // Mapujemy wynik na naszą klasę StockData
            return new StockDataResponse
            {
                Symbol = symbol,
                Name = ticker.LongName,
                CurrentPrice = (decimal)ticker.RegularMarketPrice,
                Currency = ticker.Currency,
                // Przeliczamy czas z formatu Unix na DateTime
                LastTradeTime = DateTimeOffset.FromUnixTimeSeconds(ticker.RegularMarketTime).DateTime
            };
        }
        catch (Exception ex)
        {
            // Logowanie błędu (opcjonalnie)
            throw new Exception($"Nie udało się pobrać danych dla {symbol}: {ex.Message}");
        }
    }
}
