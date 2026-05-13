using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.DTOs.HistoryDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTracker.WebAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class HistoryController : ControllerBase
{
    private readonly IHistoryService _historyService;
    private readonly IUserService _userService;

    public HistoryController(IHistoryService historyService, IUserService userService   )
    {
        _historyService = historyService;
        _userService = userService;
    }

    /// <summary>
    /// Returns the user ID if the user is authenticated; otherwise, throws an UnauthorizedAccessException.
    /// </summary>
    private int CurrentUserId => _userService.UserId ?? throw new UnauthorizedAccessException();

    /// <summary>
    /// Gets all history entries for the authenticated user. If the user is not authenticated, returns an Unauthorized response.
    /// If there are no history entries, returns an empty list.
    /// </summary>
    /// <returns>A list of history records for the authenticated user. If there are no history entries, the list will be empty.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HistoryDTO>>> GetAll()
    {
        IEnumerable<HistoryDTO> historyDtos = await _historyService.GetHistoryAsync(CurrentUserId);

        return Ok(historyDtos ?? Enumerable.Empty<HistoryDTO>());
    }

    /// <summary>
    /// Gets a single history record by its unique identifier. 
    /// </summary>
    /// <param name="historyId">The unique identifier of the history record to retrieve.</param>
    /// <returns>If the history record is found, returns an Ok response with the history data.</returns>
    [HttpGet("{historyId}")]
    public async Task<ActionResult<HistoryDTO?>> GetSingleHistoryAsync([FromRoute] int historyId)
    {
        var historyDTO = await _historyService.GetSingleHistoryElementAsync(CurrentUserId, historyId);

        if (historyDTO == null)
            return NotFound(historyDTO);

        return Ok(historyDTO);
    }

    /// <summary>
    /// Creates a new history entry using the provided data.
    /// </summary>
    /// <remarks>This method requires the user to be authenticated. The created history resource is returned in
    /// the response body upon success.</remarks>
    /// <param name="createHistoryDTO">The data transfer object containing the details of the history to create. Cannot be null.</param>
    /// <returns>A response with status code 201 (Created) containing the created history if successful; otherwise, a 401
    /// (Unauthorized) response if the user is not authenticated.</returns>
    [HttpPost("Post")]
    public async Task<ActionResult> PostAsync([FromBody] CreateHistoryDTO createHistoryDTO)
    {
        var historyId = await _historyService.InsertHistoryElementAsync(createHistoryDTO, CurrentUserId);

        var historyDTO = await _historyService.GetSingleHistoryElementAsync(CurrentUserId, historyId);

        return CreatedAtAction(nameof(GetSingleHistoryAsync), new { id = historyId }, historyDTO);
    }

    /// <summary>
    /// Deletes a specific history record identified by the provided history ID. The user must be authenticated to perform this action.
    /// </summary>
    /// <param name="historyId">The unique identifier of the history to delete. Must correspond to an existing history record.</param>
    /// <returns>An HTTP 204 No Content response if the deletion is successful; 
    /// otherwise, an HTTP 404 Not Found response if the specified history does not exist.</returns>
    [HttpDelete("{historyId}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int historyId)
    {
        var historyDTO = await _historyService.GetSingleHistoryElementAsync(CurrentUserId, historyId);

        if (historyDTO == null)
            return NotFound();

        await _historyService.DeleteSingleHistoryElementAsync(CurrentUserId, historyId);

        return NoContent();
    }

    /// <summary>
    /// Deletes all history associated with the currently authenticated user.
    /// </summary>
    /// <remarks>This action requires the user to be authenticated. All history for the current user will be
    /// permanently removed. Use with caution, as this operation cannot be undone.</remarks>
    /// <returns>An indicating the result of the operation. Returns if the user is not authenticated, 
    /// if no history is found for the user, or if the history is successfully deleted.</returns>
    [HttpDelete("DeleteAll")]
    public async Task<ActionResult> DeleteAllAsync()
    {
        var userHistoryDTOs = await _historyService.GetHistoryAsync(CurrentUserId);

        if (userHistoryDTOs == null || !userHistoryDTOs.Any())
            return NotFound();

        await _historyService.DeleteWholeHistoryAsync(CurrentUserId);

        return NoContent();
    }
}
