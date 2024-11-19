using CashFlow.Domain.Repositories;

namespace CashFlow.Infrastructure.DataAccess;

internal class UnityOfWork : IUnityOfWork
{
    private readonly CashFlowDbContext _context;
    public UnityOfWork(CashFlowDbContext context)
    {
        _context = context;
    }

    public async Task Commit() => await _context.SaveChangesAsync();
}
