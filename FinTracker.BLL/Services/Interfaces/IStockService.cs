using FinTracker.Models.Response;

namespace FinTracker.BLL.Services.Interfaces;

public interface IStockService
{
    /// <summary>
    /// Gets the current stock data for a given symbol by querying the Yahoo Finance API.
    /// </summary>
    /// <param name="symbol">The stock symbol to query.</param>
    /// <returns>The stock data for the specified symbol, or null if not found.</returns>
    /// <exception cref="ArgumentException">Thrown when the symbol is null or whitespace.</exception>
    /// <exception cref="HttpRequestException">Thrown when there is a communication error with the Yahoo Finance API.</exception>
    Task<StockDataResponse?> GetStockDataAsync(string symbol);
}
