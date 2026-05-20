using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.DTOs.PortfolioDTOs;
using FinTracker.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTracker.WebAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
/// <summary>
/// Controller responsible for portfolio-related endpoints.
/// </summary>
/// <remarks>
/// All endpoints require an authenticated user.
/// </remarks>
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PortfolioController"/> class.
    /// </summary>
    /// <param name="portfolioService">Service that handles portfolio operations.</param>
    /// <param name="userService">Service that provides information about the current user.</param>
    public PortfolioController(IPortfolioService portfolioService, IUserService userService)
    {
        _portfolioService = portfolioService;
        _userService = userService;
    }

    /// <summary>
    /// Gets the current authenticated user's identifier.
    /// </summary>
    /// <remarks>
    /// Throws <see cref="InvalidOperationException"/> when the user id is missing from the authenticated context.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when user id is not available.</exception>
    private int CurrentUserId => _userService.UserId ?? 
        throw new InvalidOperationException("User ID is missing from the authenticated context.");

    /// <summary>
    /// Retrieves the current portfolio for the authenticated user.
    /// </summary>
    /// <returns>The current portfolio view model.</returns>
    [HttpGet]
    public async Task<ActionResult<PortfolioViewModel>> GetPortfolioAsync()
    {
        PortfolioViewModel portfolioViewModel = await _portfolioService.GetCurrentPortfolioDataAsync(CurrentUserId);

        return Ok(portfolioViewModel);
    }

    /// <summary>
    /// Retrieves the portfolio history for the authenticated user.
    /// </summary>
    /// <returns>A collection of saved portfolio DTOs or NotFound if none exist.</returns>
    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<PortfolioDTO>>> GetPortfolioHistory()
    {
        var portfolioHistoryResults = await _portfolioService.GetPortfolioHistoryAsync(CurrentUserId);

        if (portfolioHistoryResults == null)
            return NotFound(portfolioHistoryResults);

        return Ok(portfolioHistoryResults);
    }

    /// <summary>
    /// Retrieves a specific saved portfolio by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the saved portfolio.</param>
    /// <returns>The portfolio DTO if found; otherwise NotFound.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<PortfolioDTO>> GetPortfolioSaveById([FromRoute] int id)
    {
        var portfolioDTO = await _portfolioService.GetPortfolioSaveByIdAsync(id);

        if (portfolioDTO == null)
            return NotFound();

        return Ok(portfolioDTO);
    }

    /// <summary>
    /// Saves the current portfolio state for the authenticated user.
    /// </summary>
    /// <returns>Created result containing the saved portfolio DTO.</returns>
    [HttpPost("save")]
    public async Task<ActionResult> SaveCurrentPortfolio()
    {
        var portfolioSaveId = await _portfolioService.SaveCurrentPortfolioDataAsync(CurrentUserId);

        var portfolioDTO = await _portfolioService.GetPortfolioSaveByIdAsync(portfolioSaveId);

        return CreatedAtAction(nameof(GetPortfolioSaveById), new { id = portfolioSaveId }, portfolioDTO);
    }

    /// <summary>
    /// Saves a historical portfolio provided in the request body.
    /// </summary>
    /// <param name="savePortfolioDTO">Data transfer object containing historical portfolio data.</param>
    /// <returns>Created result containing the saved portfolio DTO.</returns>
    [HttpPost("historical")]
    public async Task<ActionResult> SaveHistoricalPortfolio([FromBody] SavePortfolioDTO savePortfolioDTO)
    {
        var portfolioSaveId = await _portfolioService.SaveHistoricalPortfolioDataAsync(CurrentUserId, savePortfolioDTO);

        var portfolioDTO = await _portfolioService.GetPortfolioSaveByIdAsync(portfolioSaveId);

        return CreatedAtAction(nameof(GetPortfolioSaveById), new { id = portfolioSaveId }, portfolioDTO);
    }


    /// <summary>
    /// Deletes a single saved portfolio by id for the authenticated user.
    /// </summary>
    /// <param name="portfolioId">The identifier of the portfolio to delete.</param>
    /// <returns>NoContent if deleted; NotFound if the portfolio does not exist.</returns>
    [HttpDelete("{portfolioId}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int portfolioId)
    {
        try
        {
            await _portfolioService.DeleteSinglePortfolioAsync(CurrentUserId, portfolioId);
            return NoContent();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Deletes all saved portfolios for the authenticated user.
    /// </summary>
    /// <returns>NoContent if at least one row was deleted; NotFound if there were no saved portfolios.</returns>
    [HttpDelete]
    public async Task<ActionResult> DeleteAllAsync()
    {
        var deletedRows = await _portfolioService.DeleteWholePortfolioAsync(CurrentUserId);

        if(deletedRows == 0)
            return NotFound();  

        return NoContent();
    }
}
