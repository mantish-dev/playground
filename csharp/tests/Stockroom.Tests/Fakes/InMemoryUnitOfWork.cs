using Stockroom.Business.Abstractions;

namespace Stockroom.Tests.Fakes;

internal sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
