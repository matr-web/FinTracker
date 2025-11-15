using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.DTOs.CashDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinTracker.WebAPI.Controllers
{
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

        // GET: api/<CashController>
        [HttpGet("CashHistory")]
        public ActionResult<IEnumerable<CashDTO>> GetHistory()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var cashHistory = _cashService.GetCashHistory(userId);

            if (cashHistory == null)
            {
                return NotFound();
            }

            return Ok(cashHistory);
        }

        // GET api/<CashController>/5
        [HttpGet("CurrentCash")]
        public async Task<ActionResult> GetCurrentCashAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var currentCash = await _cashService.GetCurrentCashAsync(userId);

            if (currentCash == null)
            {
                return NotFound();
            }

            return Ok(currentCash);
        }

        // POST api/<CashController>
        [HttpPost("Post")]
        public async Task<ActionResult> PostAsync([FromBody] CreateCashDTO createCashDTO)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var cashId = await _cashService.InsertCashAsync(createCashDTO, userId);

            var cashDTO = await _cashService.GetSingleCashValueAsync(cashId);

            return Created($"Cash/{cashId}", cashDTO);
        }

        // DELETE api/<CashController>/5
        [HttpDelete("Delete/{cashId}")]
        public async Task<ActionResult> DeleteAsync([FromRoute] int cashId)
        {
            await _cashService.DeleteCashAsync(cashId);

            return NoContent();
        }
    }
}
