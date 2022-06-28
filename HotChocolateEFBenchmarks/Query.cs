using HotChocolateEFBenchmarks.Data;

namespace HotChocolateEFBenchmarks;

public class Query
{
    [UseProjection]
    public IQueryable<User> GetUsers(HotBenchDbContext context, int? limit = null)
    {
        IQueryable<User> query = context.Users;
        if (limit is { } validLimit)
        {
            query = query.Take(validLimit);
        }

        return query;
    }
    
    [UseProjection]
    public IQueryable<Device> GetDevices(HotBenchDbContext context, int? limit = null)
    {
        IQueryable<Device> query = context.Devices;
        if (limit is { } validLimit)
        {
            query = query.Take(validLimit);
        }

        return query;
    }
}