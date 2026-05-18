using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.DTOs.HoldingDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTracker.WebAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class HoldingsController : ControllerBase
{
    private readonly IHoldingService _holdingService;
    private readonly IUserService _userService; 

    public HoldingsController(IHoldingService holdingService, IUserService userService)
    {
        _holdingService = holdingService;
        _userService = userService;
    }

    /// <summary>
    /// Returns the user ID if the user is authenticated; otherwise, throws an UnauthorizedAccessException.
    /// </summary>
    private int CurrentUserId => _userService.UserId ?? throw new UnauthorizedAccessException();

    /// <summary>
    /// Returns a single <c>HoldingDTO</c> with the specified identifier that belongs to the current user.
    /// Uses <c>CurrentUserId</c> to ensure the holding is fetched only for the authenticated user.
    /// </summary>
    /// <param name="holdingId">The identifier of the requested holding.</param>
    /// <returns>
    /// 200 OK with the <c>HoldingDTO</c> when the holding exists and belongs to the current user;
    /// 404 Not Found when the holding does not exist or does not belong to the current user.
    /// </returns>
    /// <response code="200">Returns the requested <c>HoldingDTO</c>.</response>
    /// <response code="404">Holding not found for the current user.</response>
    [HttpGet("{holdingId}")]
    public async Task<ActionResult<HoldingDTO?>> GetHoldingByIdAsync([FromRoute] int holdingId)
    {
        var holdingDTO = await _holdingService.GetHoldingByIdAsync(CurrentUserId, holdingId);

        if (holdingDTO == null)
            return NotFound();

        return Ok(holdingDTO);
    }

    /// <summary>
    /// Creates or updates a holding for the current user.
    /// </summary>
    /// <remarks>
    /// This action calls <see cref="IHoldingService.InsertOrUpdateHoldingAsync"/> which either inserts a new holding
    /// or updates an existing one depending on the contents of <paramref name="createHoldingDTO"/>.
    /// After the operation, it returns the full <see cref="HoldingDTO"/> using <see cref="CreatedAtAction(string, object, object)"/>.
    /// </remarks>
    /// <param name="createHoldingDTO">The model containing data for creating or updating a holding.</param>
    /// <returns>
    /// - 201 Created with the location header and the created/updated <see cref="HoldingDTO"/> when successful;
    /// - 400 Bad Request if the input model is invalid (automatic model validation applies because of <see cref="ApiControllerAttribute"/>);
    /// - 401 Unauthorized if the user is not authenticated.
    /// </returns>
    /// <response code="201">Returns the created or updated <see cref="HoldingDTO"/> and Location header.</response>
    /// <response code="400">Invalid input data.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpPost]
    public async Task<ActionResult> PostAsync([FromBody] CreateHoldingDTO createHoldingDTO)
    {
        var holdingId = await _holdingService.InsertOrUpdateHoldingAsync(createHoldingDTO, CurrentUserId);

        var holdingDTO = await _holdingService.GetHoldingByIdAsync(CurrentUserId, holdingId);

        return CreatedAtAction("GetHoldingById", new { holdingId }, holdingDTO);
    }

    /// <summary>
    /// Deletes a holding identified by <paramref name="holdingId"/> that belongs to the current user.
    /// </summary>
    /// <param name="holdingId">Identifier of the holding to delete.</param>
    /// <returns>
    /// 204 No Content when deletion is successful;
    /// 404 Not Found when the holding does not exist or does not belong to the current user;
    /// 401 Unauthorized when the user is not authenticated.
    /// </returns>
    /// <response code="204">Holding has been deleted successfully.</response>
    /// <response code="404">Holding not found or not owned by the current user.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpDelete("Delete/{holdingId}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int holdingId)
    {
        var result = await _holdingService.DeleteHoldingAsync(CurrentUserId, holdingId);

        if (!result)
            return NotFound();

        return NoContent();
    }
}
