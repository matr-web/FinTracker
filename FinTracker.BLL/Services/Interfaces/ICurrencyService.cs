namespace FinTracker.BLL.Services.Interfaces;

public interface ICurrencyService
{
    Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);
}
