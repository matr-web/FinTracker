using FinTracker.DAL.Entities;

namespace FinTracker.Models.DTOs.HistoryDTOs;

public class CreateHistoryDTO
{
    public required string AssetName { get; set; }

    public string? Ticker { get; set; }

    public required string AssetType { get; set; }

    public Operation Operation { get; set; }

    public double Quantity { get; set; }

    public decimal Value { get; set; }

    public Currency Currency { get; set; }

    public decimal CurrencyPrice { get; set; }

    public string? Description { get; set; }

    public DateOnly Date { get; set; }

    public decimal Profit { get; set; }

    public decimal ROI { get; set; }

    public int UserId { get; set; }
}
