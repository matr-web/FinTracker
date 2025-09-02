using FinTracker.Models.DTOs.HoldingDTOs;

namespace FinTracker.BLL.Services.Interfaces;

public interface IHoldingService
{
    /// <summary>
    /// Get All Holdings for a given User.
    /// </summary>
    IEnumerable<HoldingDTO> GetHoldingsAsync(int userId);

    /// <summary>
    /// Add new Holding.
    /// </summary>
    Task<int> InsertHoldingAsync(CreateHoldingDTO createHoldingDTO, int userId);

    /// <summary>
    /// Delete given Holding.
    /// </summary>
    Task DeleteHoldingAsync(int holdingId);
}
