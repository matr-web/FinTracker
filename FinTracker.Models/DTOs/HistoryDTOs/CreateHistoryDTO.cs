using FinTracker.DAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace FinTracker.Models.DTOs.HistoryDTOs;

public class CreateHistoryDTO
{
    public required string AssetName { get; set; }
    public string? Ticker { get; set; }
    public AssetType AssetType { get; set; }
    public Operation Operation { get; set; }
    public double Quantity { get; set; }
    [DataType(DataType.Currency)]
    public decimal PricePerUnit { get; set; }
    public Currency Currency { get; set; }
    [DataType(DataType.Currency)]
    public decimal? CurrencyPrice { get; set; }
    public string? Description { get; set; }
    public DateOnly? Date { get; set; }
    [DataType(DataType.Currency)]
    public decimal? Profit { get; set; }
    public decimal? ROI { get; set; }
}
