using FinTracker.Models.DTOs.HoldingDTOs;

namespace FinTracker.BLL.Services.Interfaces;

public interface IHoldingService
{
    /// <summary>
    /// Get All Holdings for a given User.
    /// </summary>
    Task<IEnumerable<HoldingDTO>> GetHoldingsAsync(int userId);

    /// <summary>
    /// Get Holding with given Id.
    /// </summary>
    Task<HoldingDTO?> GetHoldingAsync(int userId);

    /// <summary>
    /// Add new Holding.
    /// </summary>
    Task<int> InsertOrUpdateHoldingAsync(CreateHoldingDTO createHoldingDTO, int userId);

    /// <summary>
    /// Delete given Holding.
    /// </summary>
    Task DeleteHoldingAsync(int holdingId);
}
