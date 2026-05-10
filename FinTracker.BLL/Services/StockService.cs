using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.Response;
using YahooFinanceApi;

namespace FinTracker.BLL.Services;

public class StockService : IStockService
{
    public async Task<StockDataResponse?> GetStockDataAsync(string symbol)
    {
        try
        {
            // Ask Yahoo Finance API for the stock data using the provided symbol
            var query = await Yahoo.Symbols(symbol)
                                   .Fields(Field.RegularMarketPrice,
                                           Field.Currency,
                                           Field.LongName,
                                           Field.RegularMarketTime)
                                   .QueryAsync();

            if (!query.ContainsKey(symbol))
            {
                return null; // Return null if the symbol is not found in the response
            }

            var ticker = query[symbol];

            // Map the data from the Yahoo Finance API to our StockDataResponse model
            return new StockDataResponse
            {
                Symbol = symbol,
                Name = ticker.LongName,
                CurrentPrice = (decimal)ticker.RegularMarketPrice,
                Currency = ticker.Currency,
                // Convert the Unix timestamp to a DateTime object
                LastTradeTime = DateTimeOffset.FromUnixTimeSeconds(ticker.RegularMarketTime).DateTime
            };
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed. For now, we rethrow it with a custom message.
            throw new Exception($"Nie udało się pobrać danych dla {symbol}: {ex.Message}");
        }
    }
}
