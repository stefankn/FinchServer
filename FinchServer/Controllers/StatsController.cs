using FinchServer.Beets;
using FinchServer.Controllers.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FinchServer.Controllers;

[ApiController]
[Route("/api/v1/stats")]
public class StatsController(BeetsConfiguration beetsConfiguration) {
    
    // - Functions

    [HttpGet]
    public ActionResult<StatsDto> Stats() {
        return beetsConfiguration.GetStats();
    }
}