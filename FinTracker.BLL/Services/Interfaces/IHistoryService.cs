using FinTracker.Models.DTOs.HistoryDTOs;

namespace FinTracker.BLL.Services.Interfaces;

public interface IHistoryService
{
    /// <summary>
    /// Retrieves the history for a given user. Returns an empty collection if the user has no history.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom to retrieve history.</param>
    /// <returns>A collection of history records for the specified user.</returns>
    Task<IEnumerable<HistoryDTO>> GetHistoryAsync(int userId);

    /// <summary>
    /// Retrieves a single history element identified by the specified history ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom to retrieve the history element.</param>
    /// <param name="historyId">The unique identifier of the history element to retrieve.</param>
    /// <returns>The history element if found; otherwise, null.</returns>
    Task<HistoryDTO?> GetSingleHistoryElementAsync(int userId, int historyId);

    /// <summary>
    /// Inserts a new history record asynchronously and returns the unique identifier of the created history element.
    /// </summary>
    /// <param name="createHistoryDTO">The data transfer object containing the details of the history to create.</param>
    /// <param name="userId">The unique identifier of the user for whom to create the history record.</param>
    /// <returns>The unique identifier of the created history element.</returns>
    Task<int> InsertHistoryElementAsync(CreateHistoryDTO createHistoryDTO, int userId);

    /// <summary>
    /// Deletes a specific history record identified by the provided history ID. The user must be authenticated to perform this action.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom to delete the history record.</param>
    /// <param name="historyId">The unique identifier of the history record to delete.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    Task<bool> DeleteSingleHistoryElementAsync(int userId, int historyId);

    /// <summary>
    /// Deletes all history records associated with the specified user. The user must be authenticated to perform this action.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom to delete all history records.</param>
    /// <returns>The number of history records that were deleted.</returns>
    Task<int> DeleteWholeHistoryAsync(int userId);
}
