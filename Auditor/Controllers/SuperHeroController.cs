using Auditor.Models;
using Auditor.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auditor.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SuperHeroController(SuperHeroService superHeroService) : ControllerBase
{
    private readonly SuperHeroService _superHeroService = superHeroService;

    [HttpGet]
    public async Task<IActionResult> GetAllHeros()
    {
        var heros = await _superHeroService.GetAllSuperHeros();
        return Ok(heros);
    }

    [HttpPost]
    public async Task<IActionResult> AddHero(SuperHero superHero)
    {
        await _superHeroService.AddSuperHero(superHero);
        return Ok();
    }
}
