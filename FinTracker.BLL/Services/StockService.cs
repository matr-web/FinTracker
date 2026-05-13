using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.Response;
using YahooFinanceApi;

namespace FinTracker.BLL.Services;

public class StockService : IStockService
{
    /// <inheritdoc cref="IStockService.GetStockDataAsync" />
    public async Task<StockDataResponse?> GetStockDataAsync(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be null or whitespace", nameof(symbol));

        var normalizedSymbol = symbol.Trim().ToUpperInvariant();

        try
        {
            // Ask Yahoo Finance API for the stock data using the provided symbol
            var query = await Yahoo.Symbols(normalizedSymbol)
                                   .Fields(Field.RegularMarketPrice,
                                           Field.Currency,
                                           Field.LongName,
                                           Field.RegularMarketTime)
                                   .QueryAsync();

            if (!query.TryGetValue(normalizedSymbol, out var ticker))
            {
                // Return null if the symbol is not found in the response
                return null;
            }

            // Map the data from the Yahoo Finance API to our StockDataResponse model
            return new StockDataResponse
            {
                Symbol = normalizedSymbol,
                Name = ticker.LongName ?? "Unknown",
                CurrentPrice = (Convert.ToDecimal(ticker.RegularMarketPrice)),
                Currency = ticker.Currency ?? "Unknown",
                // Convert the Unix timestamp to a DateTime object
                LastTradeTime = DateTimeOffset.FromUnixTimeSeconds(ticker.RegularMarketTime).DateTime
            };
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed. For now, we rethrow it with a custom message.
            throw new HttpRequestException($"Communication error with Yahoo Finance for symbol {normalizedSymbol}", ex);
        }
    }
}
