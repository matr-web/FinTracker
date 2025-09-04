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
    public HoldingsController(IHoldingService holdingService)
    {
        _holdingService = holdingService;
    }

    [HttpGet("GetAll")]
    public ActionResult<IEnumerable<HoldingDTO>> GetAll()
    {
        IEnumerable<HoldingDTO> holdingDtos = _holdingService.GetHoldings(1);

        if (holdingDtos is null)
        {
            return NotFound(holdingDtos);
        }

        return Ok(holdingDtos);
    }
}
