using HotChocolateEFBenchmarks.Data;
using Microsoft.EntityFrameworkCore;

namespace HotChocolateEFBenchmarks.Services;

public interface IInflationApplier
{
    Task ApplyOnUserDevices(int userId);
}
class FactoryInflationApplier : IInflationApplier, IAsyncDisposable, IDisposable
{
    private readonly HotBenchDbContext _context;

    public FactoryInflationApplier(IDbContextFactory<HotBenchDbContext> contextFactory)
    {
        _context = contextFactory.CreateDbContext();
    }

    public async Task ApplyOnUserDevices(int userId)
    {
        foreach (var device in await _context.Devices.Where(x => x.UserId == userId).ToArrayAsync())
        {
            device.Price *= 1.10M; // Inflation!
        }

        await _context.SaveChangesAsync();
    }

    public ValueTask DisposeAsync()
    {
        return _context.DisposeAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
public class ScopedInflationApplier : IInflationApplier
{
    private readonly HotBenchDbContext _context;

    public ScopedInflationApplier(HotBenchDbContext context)
    {
        _context = context;
    }

    public async Task ApplyOnUserDevices(int userId)
    {
        foreach (var device in await _context.Devices.Where(x => x.UserId == userId).ToArrayAsync())
        {
            device.Price *= 1.10M; // Inflation!
        }

        await _context.SaveChangesAsync();
    }
}