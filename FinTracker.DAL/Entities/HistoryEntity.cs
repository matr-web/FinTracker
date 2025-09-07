using System.ComponentModel.DataAnnotations.Schema;

namespace FinTracker.DAL.Entities;

public class HistoryEntity
{
    public int Id { get; set; }

    public required string AssetName { get; set; }

    public string? Ticker { get; set; }

    public AssetType AssetType { get; set; }

    public Operation Operation { get; set; }

    public double Quantity { get; set; }

    public decimal PricePerUnit { get; set; }

    public Currency Currency { get; set; }

    public decimal? CurrencyPrice { get; set; }

    public string? Description { get; set; }

    public DateOnly? Date { get; set; } 

    public decimal? Profit { get; set; }

    public int? ROIBps { get; set; }

    public int UserId { get; set; }

    public UserEntity? User { get; set; }

    [NotMapped]
    public decimal? ROI => ROIBps / 100m; // 750 -> 7.5%
}

public enum AssetType
{
    Stock,
    ETF,
    Bonds
}

public enum Operation
{
    Buy,
    Sell
}

public enum Currency
{
    PLN,
    EUR,
    USD,
    GBP
}
