using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.DTOs.HoldingDTOs;
using FinTracker.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinTracker.WebAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class HoldingsController : ControllerBase
{
    private readonly IHoldingService _holdingService;

    public HoldingsController(IHoldingService holdingService)
    {
        _holdingService = holdingService;
    }

    [HttpPost("Post")]
    public async Task<ActionResult> PostAsync([FromBody] CreateHoldingDTO createHoldingDTO)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var holdingId = await _holdingService.InsertOrUpdateHoldingAsync(createHoldingDTO, userId);

        var holdingDTO = await _holdingService.GetHoldingAsync(holdingId);

        return Created($"Holding/{holdingId}", holdingDTO);
    }

    [HttpDelete("Delete/{holdingId}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int holdingId)
    {
        var holdingDTO = await _holdingService.GetHoldingAsync(holdingId);

        if (holdingDTO == null)
        {
            return NotFound();
        }

        await _holdingService.DeleteHoldingAsync(holdingId);

        return NoContent();
    }
}
