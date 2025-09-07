using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.DTOs.HoldingDTOs;
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

    [HttpGet("GetAll")]
    public ActionResult<IEnumerable<HoldingDTO>> GetAll()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        IEnumerable<HoldingDTO> holdingDtos = _holdingService.GetHoldings(userId);

        if (holdingDtos is null)
        {
            return NotFound(holdingDtos);
        }

        return Ok(holdingDtos);
    }

    [HttpPost("Post")]
    public async Task<ActionResult> PostAsync([FromBody] CreateHoldingDTO createHoldingDTO)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var holdingId = await _holdingService.InsertHoldingAsync(createHoldingDTO, userId);

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
