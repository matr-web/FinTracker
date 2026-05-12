using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.DTOs.DebtDTOs;
using FinTracker.Models.DTOs.InstallmentDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinTracker.WebAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DebtController : ControllerBase
{
    private readonly IDebtService _debtService;
    public DebtController(IDebtService debtService)
    {
        _debtService = debtService;
    }

    [HttpGet("GetAll")]
    public async Task<ActionResult<IEnumerable<DebtDTO?>>> GetAll()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var debtDtos = await _debtService.GetAllDebtsAsync(userId);

        if (debtDtos is null)
        {
            return NotFound(debtDtos);
        }

        return Ok(debtDtos);
    }

    [HttpGet("Get/{debtId}")]
    public async Task<ActionResult<DebtDTO?>> GetAsync([FromRoute] int debtId)
    {
        var debtDTO = await _debtService.GetSingleDebtAsync(debtId);

        if (debtDTO is null)
        {
            return NotFound(debtDTO);
        }

        return Ok(debtDTO);
    }

    [HttpGet("GetSummed")]
    public async Task<ActionResult<IEnumerable<SummedDebtDTO?>>> GetSummed()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var summedDebtDtos = await _debtService.GetSummedDebtAsync(userId);
        if (summedDebtDtos is null)
        {
            return NotFound(summedDebtDtos);
        }

        return Ok(summedDebtDtos);
    }

    [HttpPost("PayOff/{debtId}")]
    public async Task<ActionResult> PayOffInstallmentAsync([FromRoute]int debtId,[FromBody] RepayInstallmentDTO repayInstallmentDTO)
    {
        var debtDTO = await _debtService.PayOffInstallmentAsync(debtId, repayInstallmentDTO);

        if (debtDTO == null)
        {
            return NotFound(debtDTO);
        }

        var installmentId = debtDTO!.Installments!.Last().Id; 

        return Created($"Installment/{installmentId}", debtDTO); 
    }

    [HttpPost("Post")]
    public async Task<ActionResult> PostAsync([FromBody] CreateDebtDTO createDebtDTO)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var debtId = await _debtService.InsertDebtAsync(createDebtDTO, userId);

        var debtDTO = await _debtService.GetSingleDebtAsync(debtId);

        return Created($"Debt/{debtId}", debtDTO);
    }

    [HttpDelete("Delete/{debtId}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int debtId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var debtDTO = await _debtService.GetSingleDebtAsync(debtId);

        if (debtDTO == null)
        {
            return NotFound();
        }

        await _debtService.DeleteSingleDebtAsync(userId, debtId);

        return NoContent();
    }

    [HttpDelete("DeleteAll")]
    public async Task<ActionResult> DeleteAllAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var userDebtDTOs = await _debtService.GetAllDebtsAsync(userId);

        if (userDebtDTOs == null || !userDebtDTOs.Any())
        {
            return NotFound();
        }

        await _debtService.DeleteWholeDebtAsync(userId);

        return NoContent();
    }
}
