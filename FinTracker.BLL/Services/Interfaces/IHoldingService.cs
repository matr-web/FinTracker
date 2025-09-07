using FinTracker.Models.DTOs.HoldingDTOs;

namespace FinTracker.BLL.Services.Interfaces;

public interface IHoldingService
{
    /// <summary>
    /// Get All Holdings for a given User.
    /// </summary>
    IEnumerable<HoldingDTO> GetHoldings(int userId);

    /// <summary>
    /// Get Holding with given Id.
    /// </summary>
    Task<HoldingDTO?> GetHoldingAsync(int userId);

    /// <summary>
    /// Add new Holding.
    /// </summary>
    Task<int> InsertHoldingAsync(CreateHoldingDTO createHoldingDTO, int userId);

    /// <summary>
    /// Delete given Holding.
    /// </summary>
    Task DeleteHoldingAsync(int holdingId);
}
