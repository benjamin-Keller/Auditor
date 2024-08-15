using Auditor.Data;
using Auditor.Models;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Services;

public class SuperHeroService(DataContext context)
{
    private readonly DataContext _context = context;

    public async Task<List<SuperHero>> GetAllSuperHeros()
    {
        return await _context.SuperHeroes.ToListAsync();
    }

    public async Task AddSuperHero(SuperHero superHero)
    {
        await _context.SuperHeroes.AddAsync(superHero);

        await _context.SaveChangesAsync();
    }
}
