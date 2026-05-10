using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.DTOs.CashDTOs;
using FinTracker.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FinTracker.WebAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CashController : ControllerBase
{
    private readonly ICashService _cashService;

    public CashController(ICashService cashService)
    {
        _cashService = cashService;
    }

    /// <summary>
    /// Fetches the user ID from the claims of the authenticated user. 
    /// This property is used to identify the user making the request 
    /// and to ensure that cash data is retrieved or modified for the correct user. 
    /// </summary>
    protected int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET: api/<CashController>
    /// <summary>
    /// Get cash history for a user. You need to specify the type of cash and optionally the period of time (in months) 
    /// for which you want to retrieve the history. If the period of time is not specified, it will return all history for the specified cash type.
    /// </summary>
    /// <param name="cashType">The type of cash for which to retrieve the history.</param>
    /// <param name="periodOfTime">The period of time (in months) for which to retrieve the history. If not specified, all history will be returned.</param>
    /// <returns>A list of cash history records.</returns>
    [HttpGet("GetHistory/{cashType}")]
    public async Task<ActionResult> GetHistoryAsync(
    [FromRoute] CashType cashType,
    [FromQuery, Range(0, int.MaxValue)] int? periodOfTime)
    {
        var cashHistory = await _cashService.GetCashHistoryAsync(UserId, cashType, periodOfTime);

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
        var currentCash = await _cashService.GetCurrentCashAsync(UserId, cashType);

        if (currentCash == null)
            return NotFound();

        return Ok(currentCash);
    }

    /// <summary>
    /// Retrieves a single cash record by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the cash record to retrieve.</param>
    /// <returns>An <see cref="ActionResult{CashDTO}"/> containing the cash record if found; otherwise, a NotFound result.</returns>
    [HttpGet("GetCashById/{id}")]
    public async Task<ActionResult<CashDTO>> GetSingleCashById(int id)
    {
        var cashDTO = await _cashService.GetSingleCashValueAsync(id);

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
    [HttpPost("PostCash")]
    public async Task<ActionResult> PostAsync([FromBody] CreateCashDTO createCashDTO)
    {
        var cashId = await _cashService.InsertCashAsync(UserId, createCashDTO);

        var cashDTO = await _cashService.GetSingleCashValueAsync(cashId);

        return CreatedAtAction(nameof(GetSingleCashById), new { id = cashId }, cashDTO);
    }

    // DELETE api/<CashController>/5
    /// <summary>
    /// Delete a single cash record by its unique identifier and user ID. 
    /// </summary>
    /// <param name="cashId">The unique identifier of the cash record to delete.</param>
    /// <returns>No content if the deletion is successful, or NotFound if a record with the specified identifier and user ID does not exist.</returns>
    [HttpDelete("Delete/{cashId}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int cashId)
    {
        var result = await _cashService.DeleteCashAsync(UserId, cashId);

        if (!result)
            return NotFound();

        return NoContent();
    }
}
