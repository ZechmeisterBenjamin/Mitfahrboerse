using Mitfahrboerse.Interfaces;
using Mitfahrboerse.Models;
using Microsoft.EntityFrameworkCore;

namespace Mitfahrboerse.Services;

public class BalanceService : IBalanceService
{
    private readonly MitfahrboerseDbContext _context;
    public BalanceService(MitfahrboerseDbContext context)
    {
        _context = context;
    }
    public Task<int> GetCurrentBalanceAsync(string id)
    {
        return Task.FromResult(_context.t_People.Where(p => p.PersonId == id).FirstOrDefaultAsync().Result.Points);
    }
}