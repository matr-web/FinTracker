using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.DTOs.CashDTOs;
using FinTracker.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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
        /// <summary>
        /// Get cash history for a user. You need to specify the type of cash and optionally the period of time (in months) 
        /// for which you want to retrieve the history. If the period of time is not specified, it will return all history for the specified cash type.
        /// </summary>
        /// <param name="cashType">The type of cash for which to retrieve the history.</param>
        /// <param name="periodOfTime">The period of time (in months) for which to retrieve the history. If not specified, all history will be returned.</param>
        /// <returns>A list of cash history records.</returns>
        [HttpGet("GetHistory/{cashType}")]
        public async Task<ActionResult<IEnumerable<CashDTO>>> GetHistoryAsync(
            [FromRoute][EnumDataType(typeof(CashType))] CashType cashType,
            [FromQuery] int? periodOfTime)
        {
            if(periodOfTime < 0)
            {
                return BadRequest("Period of time cannot be negative.");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var cashHistory = await _cashService.GetCashHistoryAsync(userId, cashType, periodOfTime);

            if (cashHistory == null)
            {
                return NotFound();
            }

            return Ok(cashHistory);
        }

        // GET api/<CashController>/5
        [HttpGet("CurrentCash")]
        public async Task<ActionResult> GetCurrentCashAsync([FromQuery][EnumDataType(typeof(CashType))] CashType cashType)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var currentCash = await _cashService.GetCurrentCashAsync(userId, cashType);

            if (currentCash == null)
            {
                return NotFound();
            }

            return Ok(currentCash);
        }

        // POST api/<CashController>
        [HttpPost("PostCash")]
        public async Task<ActionResult> PostAsync([FromBody] CreateCashDTO createCashDTO)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var cashId = await _cashService.InsertCashAsync(userId, createCashDTO);

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
