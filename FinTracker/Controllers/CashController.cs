using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.DTOs.CashDTOs;
using FinTracker.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FinTracker.WebAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CashController : ControllerBase
{
    private readonly ICashService _cashService;
    private readonly IUserService _userService;

    public CashController(ICashService cashService, IUserService userService)
    {
        _cashService = cashService;
        _userService = userService;
    }

    /// <summary>
    /// Returns the user ID if the user is authenticated; otherwise, throws an UnauthorizedAccessException.
    /// </summary>
    private int CurrentUserId => _userService.UserId ?? throw new UnauthorizedAccessException();

    // GET: api/<CashController>
    /// <summary>
    /// Get cash history for a user. You need to specify the type of cash and optionally the period of time (in months) 
    /// for which you want to retrieve the history. If the period of time is not specified, it will return all history for the specified cash type.
    /// </summary>
    /// <param name="cashType">The type of cash for which to retrieve the history.</param>
    /// <param name="monthsCount">The period of time (in months) for which to retrieve the history. If not specified, all history will be returned.</param>
    /// <returns>A list of cash history records.</returns>
    [HttpGet("GetHistory/{cashType}")]
    public async Task<ActionResult> GetHistoryAsync(
    [FromRoute] CashType cashType,
    [FromQuery, Range(0, int.MaxValue)] int? monthsCount)
    {
        var cashHistory = await _cashService.GetCashHistoryAsync(CurrentUserId, cashType, monthsCount);

        return Ok(cashHistory ?? Enumerable.Empty<CashDTO>());
    }

    // GET api/<CashController>/5
    /// <summary>
    /// Get current cash value for a user. You need to specify the type of cash for which you want to retrieve the current value.
    /// </summary>
    /// <param name="cashType">The type of cash(SavingAccount or PPK) for which to retrieve the current value.</param>
    /// <returns>The current cash value for the specified type.</returns>
    [HttpGet("CurrentCash")]
    public async Task<ActionResult> GetCurrentCashAsync([FromQuery][EnumDataType(typeof(CashType))] CashType cashType)
    {
        var currentCash = await _cashService.GetCurrentCashAsync(CurrentUserId, cashType);

        if (currentCash == null)
            return NotFound();

        return Ok(currentCash);
    }

    /// <summary>
    /// Retrieves a single cash record by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the cash record to retrieve.</param>
    /// <returns>An <see cref="ActionResult{CashDTO}"/> containing the cash record if found; otherwise, a NotFound result.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<CashDTO>> GetSingleCashByIdAsync(int id)
    {
        var cashDTO = await _cashService.GetSingleCashValueAsync(CurrentUserId, id);

        if (cashDTO == null)
            return NotFound();

        return Ok(cashDTO);
    }

    // POST api/<CashController>
    /// <summary>
    /// Post new cash value for a user. You need to specify the type of cash, the amount 
    /// and optionally the date for which you want to add the cash value.
    /// </summary>
    /// <param name="createCashDTO">An object containing the details of the cash record to insert, 
    /// including amount, cash type, and optional date.</param>
    /// <returns>The newly created cash record.</returns>
    [HttpPost]
    public async Task<ActionResult> PostAsync([FromBody] CreateCashDTO createCashDTO)
    {
        var cashId = await _cashService.InsertCashAsync(CurrentUserId, createCashDTO);

        var cashDTO = await _cashService.GetSingleCashValueAsync(CurrentUserId, cashId);

        return CreatedAtAction("GetSingleCashById", new { id = cashId }, cashDTO);
    }

    // DELETE api/<CashController>/5
    /// <summary>
    /// Delete a single cash record by its unique identifier and user ID. 
    /// </summary>
    /// <param name="cashId">The unique identifier of the cash record to delete.</param>
    /// <returns>No content if the deletion is successful, or NotFound if a record with the specified identifier and user ID does not exist.</returns>
    [HttpDelete("{cashId}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int cashId)
    {
        var result = await _cashService.DeleteCashAsync(CurrentUserId, cashId);

        if (!result)
            return NotFound();

        return NoContent();
    }
}
