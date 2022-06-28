using GreenDonut;
using HotChocolateEFBenchmarks.Data;
using Microsoft.EntityFrameworkCore;

namespace HotChocolateEFBenchmarks.Services;

public interface ITotalWealthDataLoader : IDataLoader<int, decimal>
{
    public static Task<Dictionary<int, decimal>> MakeDictionaryAsync(IReadOnlyList<int> keys,
        IQueryable<Device> devices)
    {
        var query = from device in devices
            where keys.Contains(device.UserId)
            group device by device.UserId
            into grouping
            select new
            {
                Id = grouping.Key,
                Sum = grouping.Sum(x => x.Price)
            };

        return query.ToDictionaryAsync(x => x.Id, x => x.Sum);
    }
}

public class ScopedTotalWealthDataLoader : BatchDataLoader<int, decimal>, ITotalWealthDataLoader
{
    private readonly HotBenchDbContext _context;

    public ScopedTotalWealthDataLoader(HotBenchDbContext context, IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _context = context;
    }

    protected override async Task<IReadOnlyDictionary<int, decimal>> LoadBatchAsync(IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        return await ITotalWealthDataLoader.MakeDictionaryAsync(keys, _context.Devices);
    }
}

public class FactoryTotalWealthDataLoader : BatchDataLoader<int, decimal>, ITotalWealthDataLoader, IAsyncDisposable
{
    private readonly HotBenchDbContext _context;

    public FactoryTotalWealthDataLoader(IDbContextFactory<HotBenchDbContext> contextFactory, IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _context = contextFactory.CreateDbContext();
    }

    protected override async Task<IReadOnlyDictionary<int, decimal>> LoadBatchAsync(IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        return await ITotalWealthDataLoader.MakeDictionaryAsync(keys, _context.Devices);
    }

    public ValueTask DisposeAsync() => _context.DisposeAsync();
}