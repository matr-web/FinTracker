using FinTracker.Models.DTOs.HistoryDTOs;

namespace FinTracker.BLL.Services.Interfaces;

public interface IHistoryService
{
    /// <summary>
    /// Get the whole buy/sell history for a given user.
    /// </summary>
    IEnumerable<HistoryDTO> GetHistory(int userId);

    /// <summary>
    /// Get a single history element with given Id.
    /// </summary>
    Task<HistoryDTO?> GetHistoryElementAsync(int historyId);

    /// <summary>
    /// Add new element to the history.
    /// </summary>
    Task<int> InsertHistoryElementAsync(CreateHistoryDTO createHistoryDTO, int userId);

    /// <summary>
    /// Delete single history element.
    /// </summary>
    Task DeleteSingleHistoryElementAsync(int historyId);

    /// <summary>
    /// Delete the whole history for a given user.
    /// </summary>
    Task DeleteWholeHistoryAsync(int userId);
}
