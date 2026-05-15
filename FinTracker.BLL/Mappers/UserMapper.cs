using FinTracker.BLL.Utils;
using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.CashDTOs;
using FinTracker.Models.DTOs.DebtDTOs;
using FinTracker.Models.DTOs.HistoryDTOs;
using FinTracker.Models.DTOs.HoldingDTOs;
using FinTracker.Models.DTOs.UserDTOs;
using System.Linq.Expressions;

namespace FinTracker.BLL.Mappers;

public class UserMapper
{
    /// <summary>
    /// Gets an expression that projects a UserEntity to a UserDTO for use in LINQ queries.
    /// </summary>
    /// <remarks>This projection can be used with Entity Framework or other LINQ providers to efficiently
    /// select only the required fields from the data source.</remarks>
    public static Expression<Func<UserEntity, UserDTO>> Projection =>
    u => new UserDTO
    {
        Id = u.Id,
        Username = u.Username,
        Email = u.Email,
        PasswordHash = u.PasswordHash
    };

    /// <summary>
    /// Gets an expression that projects a UserEntity to a UserDTO including all related entities (History, Holdings, Debts, Cash).
    /// </summary>
    public static Expression<Func<UserEntity, UserDTO>> FullProjection =>
    u => new UserDTO
    {
        Id = u.Id,
        Username = u.Username,
        Email = u.Email,
        PasswordHash = u.PasswordHash,

        History = u.History != null ? u.History.Select(h => new HistoryDTO
        {
            Id = h.Id,
            AssetName = h.AssetName,
            Ticker = h.Ticker,
            AssetType = h.AssetType,
            Operation = h.Operation,
            Quantity = h.Quantity,
            PricePerUnit = h.PricePerUnit,
            CurrencyCode = h.CurrencyCode,
            CurrencyPrice = h.CurrencyPrice,
            Description = h.Description,
            Date = h.Date,
            Profit = h.Profit
        }).ToList() : null,

        Holdings = u.Holdings != null ? u.Holdings.Select(h => new HoldingDTO
        {
            Id = h.Id,
            TickerSymbol = h.TickerSymbol,
            StockName = h.StockName,
            Quantity = h.Quantity,
            BuyPrice = h.BuyPrice,
            CurrencyCode = h.CurrencyCode,
            UserId = h.UserId
        }).ToList() : null,

        Debts = u.Debts != null ? u.Debts.Select(d => new DebtDTO
        {
            Id = d.Id,
            UserId = d.UserId,
            LoanPurpose = d.LoanPurpose,
            Amount = d.Amount,
            InterestRateProcentage = d.InterestRateProcentage,
            NumberOfInstallments = d.NumberOfInstallments,
            InstallmentAmount = d.InstallmentAmount,
            Date = d.Date,
        }).ToList() : null,

        Cash = u.Cash != null ? u.Cash.Select(c => new CashDTO
        {
            Id = c.Id,
            UserId = c.UserId,
            Amount = c.Amount,
            CashType = c.CashType,
            Date = c.Date,
        }).ToList() : null
    };
}