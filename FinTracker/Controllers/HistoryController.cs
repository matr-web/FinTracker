using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.DTOs.HistoryDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinTracker.WebAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class HistoryController : ControllerBase
{
    private readonly IHistoryService _historyService;
    public HistoryController(IHistoryService historyService)
    {
        _historyService = historyService;
    }

    [HttpGet("GetAll")]
    public ActionResult<IEnumerable<HistoryDTO>> GetAll()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        IEnumerable<HistoryDTO> historyDtos = _historyService.GetHistory(userId);

        if (historyDtos is null)
        {
            return NotFound(historyDtos);
        }

        return Ok(historyDtos);
    }

    [HttpPost("Post")]
    public async Task<ActionResult> PostAsync([FromBody] CreateHistoryDTO createHistoryDTO)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var historyId = await _historyService.InsertHistoryElementAsync(createHistoryDTO, userId);

        var historyDTO = await _historyService.GetHistoryElementAsync(historyId);

        return Created($"History/{historyId}", historyDTO);
    }

    [HttpDelete("Delete/{historyId}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int historyId)
    {
        var historyDTO = await _historyService.GetHistoryElementAsync(historyId);

        if (historyDTO == null)
        {
            return NotFound();
        }

        await _historyService.DeleteSingleHistoryElementAsync(historyId);

        return NoContent();
    }
}
