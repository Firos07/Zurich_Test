using Claims.Application.Abstractions;
using Claims.Application.DTOs;
using Claims.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

[ApiController]
[Route("api/claims")]
public class ClaimsController : ControllerBase
{
    private readonly IClaimService _service;

    public ClaimsController(IClaimService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClaimResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetClaims(
        [FromQuery] ClaimStatus? status,
        [FromQuery] string? policyNumber,
        CancellationToken ct)
    {
        if (status.HasValue && !Enum.IsDefined(typeof(ClaimStatus), status.Value))
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Estado no válido",
                Detail = $"El estado '{status}' no es válido. Valores permitidos: OPEN, IN_REVIEW, CLOSED."
            });

        var claims = await _service.GetClaimsAsync(status, policyNumber, ct);
        return Ok(claims);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClaim(int id, CancellationToken ct)
    {
        var claim = await _service.GetClaimAsync(id, ct);
        return Ok(claim);
    }

    [HttpGet("{id:int}/history")]
    [ProducesResponseType(typeof(IReadOnlyList<ClaimStatusHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusHistory(int id, CancellationToken ct)
    {
        var history = await _service.GetStatusHistoryAsync(id, ct);
        return Ok(history);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClaimRequest request, CancellationToken ct)
    {
        var claim = await _service.CreateClaimAsync(request, ct);
        return CreatedAtAction(nameof(GetClaim), new { id = claim.Id }, claim);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClaimRequest request, CancellationToken ct)
    {
        var claim = await _service.UpdateClaimAsync(id, request, ct);
        return Ok(claim);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteClaimAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeClaimStatusRequest request, CancellationToken ct)
    {
        var claim = await _service.ChangeStatusAsync(id, request, ct);
        return Ok(claim);
    }
}