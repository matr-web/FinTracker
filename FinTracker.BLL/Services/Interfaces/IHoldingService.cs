using FinTracker.Models.DTOs.HoldingDTOs;

namespace FinTracker.BLL.Services.Interfaces;

public interface IHoldingService
{
    /// <summary>
    /// Get a specific Holding by its Id for the given user. If the holding is not found, return null.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="holdingId">The ID of the holding to retrieve</param>
    /// <returns>The requested holding data transfer object or null if not found</returns>
    Task<HoldingDTO?> GetHoldingByIdAsync(int userId, int holdingId);

    /// <summary>
    /// Insert a new Holding or update an existing one for the given user. 
    /// If the HoldingDTO contains an Id, it will attempt to update the existing holding; otherwise, 
    /// it will create a new one. The method returns the Id of the inserted or updated holding.
    /// </summary>
    /// <param name="createHoldingDTO">The data transfer object for the holding to insert or update</param>
    /// <param name="userId">The ID of the user</param>
    /// <returns>The ID of the inserted or updated holding</returns>
    Task<int> InsertOrUpdateHoldingAsync(CreateHoldingDTO createHoldingDTO, int userId);

    /// <summary>
    /// Delete a specific Holding by its Id for the given user. The method returns true if the holding was successfully deleted.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="holdingId">The ID of the holding to delete</param>
    /// <returns>true if the holding was successfully deleted.</returns>
    Task<bool> DeleteHoldingAsync(int userId, int holdingId);
}
    