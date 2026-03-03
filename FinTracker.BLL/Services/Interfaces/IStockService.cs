using FinTracker.Models.Response;

namespace FinTracker.BLL.Services.Interfaces;

public interface IStockService
{
    Task<StockDataResponse?> GetStockDataAsync(string symbol);
}
