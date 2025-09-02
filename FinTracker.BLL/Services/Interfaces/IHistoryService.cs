using FinTracker.Models.DTOs.HistoryDTOs;

namespace FinTracker.BLL.Services.Interfaces;

public interface IHistoryService
{
    /// <summary>
    /// Get the whole buy/sell history for a given user.
    /// </summary>
    IEnumerable<HistoryDTO> GetUserHistory(int userId);

    /// <summary>
    /// Add new element to the history.
    /// </summary>
    Task<int> InsertHistoryElementAsync(CreateHistoryDTO createHistoryDTO, int userId);

    /// <summary>
    /// Delete single history element for a given user.
    /// </summary>
    Task DeleteSingleHistoryElementAsync(int historyId);

    /// <summary>
    /// Delete the whole history for a given user.
    /// </summary>
    Task DeleteWholeHistoryAsync(int userId);
}
