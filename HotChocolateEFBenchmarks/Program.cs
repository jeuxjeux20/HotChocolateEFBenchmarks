// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolateEFBenchmarks;
using HotChocolateEFBenchmarks.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

async Task TestPlayground()
{
    var servicesCollection = new ServiceCollection();
    ServicesConfiguration.ConfigureServices(servicesCollection, DbContextKind.Resolver);
    var services = servicesCollection.BuildServiceProvider();
    var executorResolver = services.GetRequiredService<IRequestExecutorResolver>();

    IRequestExecutor executor = await executorResolver.GetRequestExecutorAsync();
    IExecutionResult result = await executor.ExecuteAsync(new QueryRequest(new QuerySourceText(@"
mutation {
    applyInflation(userId: 1) {
    }
}")));

    if (result is IQueryResult cr && cr.Errors is { Count: > 0 })
    {
        throw new InvalidOperationException("The request failed.");
    }

    if (result is IAsyncDisposable d)
    {
        await d.DisposeAsync();
    }

    Console.WriteLine(result.ToJson());
}

async Task EnsureDatabaseSeeded()
{
    var servicesCollection = new ServiceCollection();
    ServicesConfiguration.ConfigureServices(servicesCollection, DbContextKind.Resolver);
    var services = servicesCollection.BuildServiceProvider();
    
    await using var scope = services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<HotBenchDbContext>();

    await context.Database.EnsureCreatedAsync();
    await context.Database.MigrateAsync();
    if (await context.Users.CountAsync() < HotBenchDbContext.UserCount || 
        await context.Devices.CountAsync() < HotBenchDbContext.UserCount * HotBenchDbContext.DevicesPerUser)
    {
        Console.WriteLine("The HotBenchDbContext is getting seeded...");
        // Remove everything and seed.
        await context.Reset();
    }
}

await EnsureDatabaseSeeded();
if (args.Length != 0 && args[0].Equals("playground", StringComparison.InvariantCultureIgnoreCase))
{
    await TestPlayground();
}
else
{
    BenchmarkRunner.Run<HotBenchmark>();
}