using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.DTOs.PortfolioDTOs;
using FinTracker.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinTracker.WebAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;
    public PortfolioController(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    [HttpGet("GetPortfolio")]
    public async Task<ActionResult<PortfolioViewModel>> GetPortfolioAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        PortfolioViewModel portfolioViewModel = await _portfolioService.GetPortfolioAsync(userId);

        if (portfolioViewModel is null)
        {
            return NotFound(portfolioViewModel);
        }

        return Ok(portfolioViewModel);
    }

    [HttpGet("GetPortfolioHistory")]
    public ActionResult<IQueryable<PortfolioDTO>> GetPortfolioHistory()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var portfolioHistoryResults = _portfolioService.GetPortfolioHistory(userId);

        if (portfolioHistoryResults is null)
        {
            return NotFound(portfolioHistoryResults);
        }

        return Ok(portfolioHistoryResults);
    }

    [HttpPost("SaveCurrentPortfolio")]
    public async Task<ActionResult> SaveCurrentPortfolio()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var portfolioSaveId = await _portfolioService.SaveCurrentPortfolioDataAsync(userId);

        var portfolioDTO = await _portfolioService.GetPortfolioSaveByIdAsync(portfolioSaveId);

        return Created($"Portfolio/{portfolioSaveId}", portfolioDTO);
    }

    [HttpPost("SaveHistoricalPortfolio")]
    public async Task<ActionResult> SaveHistoricalPortfolio([FromBody] SavePortfolioDTO savePortfolioDTO)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var portfolioSaveId = await _portfolioService.SaveHistoricalPortfolioDataAsync(userId, savePortfolioDTO);

        var portfolioDTO = await _portfolioService.GetPortfolioSaveByIdAsync(portfolioSaveId);

        return Created($"Portfolio/{portfolioSaveId}", portfolioDTO);
    }
}
