namespace FinTracker.BLL.Services.Interfaces;

public interface ICurrencyService
{
    /// <summary>
    /// Gets the exchange rate between two currencies using the Frankfurter API. 
    /// </summary>
    /// <param name="fromCurrency">The currency to convert from.</param>
    /// <param name="toCurrency">The currency to convert to.</param>
    /// <returns>The exchange rate between the two currencies.</returns>
    Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);
}
