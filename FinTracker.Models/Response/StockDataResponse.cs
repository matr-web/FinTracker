namespace FinTracker.Models.Response;

public class StockDataResponse
{
    public string Symbol { get; set; } = default!;
    public string Name { get; set; } = default!;
    public decimal CurrentPrice { get; set; }
    public string Currency { get; set; } = default!;
    public DateTime LastTradeTime { get; set; }
}
