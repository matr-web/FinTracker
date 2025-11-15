using FinTracker.Models.DTOs.CashDTOs;

namespace FinTracker.BLL.Services.Interfaces;

public interface ICashService
{
    /// <summary>
    /// Get whole cash history for a given user.
    /// </summary>
    IQueryable<CashDTO?> GetCashHistory(int userId);

    /// <summary>
    /// Get current cash value for a given user.
    /// </summary>
    Task<CashDTO?> GetCurrentCashAsync(int userId);

    /// <summary>
    /// Get given cash value for a given user.
    /// </summary>
    Task<CashDTO?> GetSingleCashValueAsync(int cashId);

    /// <summary>
    /// Add new Cash value.
    /// </summary>
    Task<int> InsertCashAsync(CreateCashDTO createCashDTO, int userId);

    /// <summary>
    /// Delete single debt.
    /// </summary>
    Task DeleteCashAsync(int cashId);
}
