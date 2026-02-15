using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mitfahrboerse.Services;

namespace Mitfahrboerse.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PointsApiController : ControllerBase
{
    private readonly IPointService _pointService;
    private readonly ILogger<PointsApiController> _logger;
    private readonly IConfiguration _configuration;

    public PointsApiController(IPointService pointService, ILogger<PointsApiController> logger, IConfiguration configuration)
    {
        _pointService = pointService;
        _logger = logger;
        _configuration = configuration;
    }

    private bool ValidateApiKey(string? providedKey)
    {
        var configuredKey = _configuration["PointsApi:ApiKey"];
        return !string.IsNullOrEmpty(configuredKey) && configuredKey == providedKey;
    }

    [HttpPost("award-points")]
    [AllowAnonymous]
    public async Task<IActionResult> AwardPointsForPastRides()
    {
        if (!Request.Headers.TryGetValue("X-API-Key", out var apiKey))
        {
            _logger.LogWarning("Missing X-API-Key header");
            return Unauthorized(new { error = "Missing X-API-Key header" });
        }

        if (!ValidateApiKey(apiKey.ToString()))
        {
            _logger.LogWarning("Invalid API key provided");
            return Unauthorized(new { error = "Invalid API key" });
        }

        try
        {
            _logger.LogInformation("Starting point award process...");
            await _pointService.AwardPointsForPastRidesAsync();
            return Ok(new { message = "Points awarded successfully for past rides." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error awarding points: {ex.Message}");
            return StatusCode(500, new { error = "Failed to award points", details = ex.Message });
        }
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "Points API is healthy" });
    }
}
