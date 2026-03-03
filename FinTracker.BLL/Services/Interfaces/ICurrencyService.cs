namespace FinTracker.BLL.Services.Interfaces;

public interface ICurrencyService
{
    Task<decimal> GetExchangeRateFrankfurterAsync(string fromCurrency, string toCurrency);
}
