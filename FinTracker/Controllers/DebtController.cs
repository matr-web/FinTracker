using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.DTOs.DebtDTOs;
using FinTracker.Models.DTOs.InstallmentDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FinTracker.WebAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DebtController : ControllerBase
{
    private readonly IDebtService _debtService;
    private readonly IUserService _userService;
    public DebtController(IDebtService debtService, IUserService userService)
    {
        _debtService = debtService;
        _userService = userService;
    }

    /// <summary>
    /// Returns the user ID if the user is authenticated; otherwise, throws an UnauthorizedAccessException.
    /// </summary>
    private int CurrentUserId => _userService.UserId ?? throw new UnauthorizedAccessException();

    /// <summary>
    /// Gets all debts for the authenticated user. If the user is not authenticated, returns an Unauthorized response.
    /// If there are no debts, returns an empty list.
    /// </summary>
    /// <returns>A list of debt records for the authenticated user. If there are no debts, the list will be empty.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DebtDTO?>>> GetAll()
    {
        var debtDtos = await _debtService.GetAllDebtsAsync(CurrentUserId);

        return Ok(debtDtos ?? Enumerable.Empty<DebtDTO>());
    }

    /// <summary>
    /// Gets a single debt record by its unique identifier. 
    /// </summary>
    /// <param name="debtId">The unique identifier of the debt record to retrieve.</param>
    /// <returns>If the debt is found, returns an Ok response with the debt data.</returns>
    [HttpGet("{debtId}")]
    public async Task<ActionResult<DebtDTO?>> GetSingleDebtByIdAsync([FromRoute] int debtId)
    {
        var debtDTO = await _debtService.GetSingleDebtAsync(CurrentUserId, debtId);

        if (debtDTO == null)
            return NotFound(debtDTO);

        return Ok(debtDTO);
    }

    /// <summary>
    /// Retrieves a collection of summed debt records for the currently authenticated user.
    /// </summary>
    /// <returns>Collection of objects representing the user's summed debts.</returns>
    [HttpGet("GetSummed")]
    public async Task<ActionResult<IEnumerable<SummedDebtDTO?>>> GetSummed([FromQuery, Range(0, int.MaxValue)] int? monthsCount)
    {
        var summedDebtDtos = await _debtService.GetSummedDebtAsync(CurrentUserId, monthsCount);

        if (summedDebtDtos == null)
            return NotFound(summedDebtDtos);

        return Ok(summedDebtDtos);
    }

    /// <summary>
    /// Processes a request to pay off an installment for a specified debt and returns the updated debt information.
    /// </summary>
    /// <param name="debtId">The unique identifier of the debt for which the installment is to be paid off. Must correspond to an existing
    /// debt.</param>
    /// <param name="repayInstallmentDTO">An object containing the details of the installment repayment, such as payment amount and date. Cannot be null.</param>
    /// <returns>An HTTP 201 Created response containing the updated debt information if the installment is successfully paid
    /// off; otherwise, an HTTP 404 Not Found response if the specified debt does not exist.</returns>
    [HttpPost("PayOff/{debtId}")]
    public async Task<ActionResult> PayOffInstallmentAsync([FromRoute]int debtId,[FromBody] RepayInstallmentDTO repayInstallmentDTO)
    {
        var debtDTO = await _debtService.PayOffInstallmentAsync(CurrentUserId, debtId, repayInstallmentDTO);

        if (debtDTO == null)
            return NotFound(debtDTO);

        var installmentId = debtDTO!.Installments!.Last().Id;

        return CreatedAtAction("GetSingleDebtById", new { id = debtId }, debtDTO);
    }

    /// <summary>
    /// Creates a new debt entry using the provided data.
    /// </summary>
    /// <remarks>This method requires the user to be authenticated. The created debt resource is returned in
    /// the response body upon success.</remarks>
    /// <param name="createDebtDTO">The data transfer object containing the details of the debt to create. Cannot be null.</param>
    /// <returns>A response with status code 201 (Created) containing the created debt if successful; otherwise, a 401
    /// (Unauthorized) response if the user is not authenticated.</returns>
    [HttpPost]
    public async Task<ActionResult> PostAsync([FromBody] CreateDebtDTO createDebtDTO)
    {
        var debtId = await _debtService.InsertDebtAsync(createDebtDTO, CurrentUserId);

        var debtDTO = await _debtService.GetSingleDebtAsync(CurrentUserId, debtId);

        return CreatedAtAction("GetSingleDebtById", new { id = debtId }, debtDTO);
    }

    /// <summary>
    /// Deletes a specific debt record identified by the provided debt ID. The user must be authenticated to perform this action.
    /// </summary>
    /// <param name="debtId">The unique identifier of the debt to delete. Must correspond to an existing debt.</param>
    /// <returns>An HTTP 204 No Content response if the deletion is successful; 
    /// otherwise, an HTTP 404 Not Found response if the specified debt does not exist.</returns>
    [HttpDelete("{debtId}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int debtId)
    {
        var debtDTO = await _debtService.GetSingleDebtAsync(CurrentUserId, debtId);

        if (debtDTO == null)
            return NotFound();

        await _debtService.DeleteSingleDebtAsync(CurrentUserId, debtId);

        return NoContent();
    }

    /// <summary>
    /// Deletes all debts associated with the currently authenticated user.
    /// </summary>
    /// <remarks>This action requires the user to be authenticated. All debts for the current user will be
    /// permanently removed. Use with caution, as this operation cannot be undone.</remarks>
    /// <returns>An indicating the result of the operation. Returns if the user is not authenticated, 
    /// if no debts are found for the user, or if the debts are successfully deleted.</returns>
    [HttpDelete("DeleteAll")]
    public async Task<ActionResult> DeleteAllAsync()
    {
        var userDebtDTOs = await _debtService.GetAllDebtsAsync(CurrentUserId);

        if (userDebtDTOs == null || !userDebtDTOs.Any())
            return NotFound();

        await _debtService.DeleteWholeDebtAsync(CurrentUserId);

        return NoContent();
    }
}
