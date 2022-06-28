using HotChocolateEFBenchmarks.Services;

namespace HotChocolateEFBenchmarks;

public class Mutations
{
    public async Task<bool> ApplyInflation(int userId, IInflationApplier inflationApplier)
    {
        await inflationApplier.ApplyOnUserDevices(userId);
        return true;
    }
}