namespace Mitfahrboerse.Interfaces;

public interface IBalanceService
{
    Task<int> GetCurrentBalanceAsync(string id);
}