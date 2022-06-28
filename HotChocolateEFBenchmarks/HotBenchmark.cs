using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using HotChocolate.Execution;
using HotChocolateEFBenchmarks.Data;
using Microsoft.Extensions.DependencyInjection;

namespace HotChocolateEFBenchmarks;

[MemoryDiagnoser]
[Config(typeof(HotBenchmarkConfig))]
public class HotBenchmark
{
    private IRequestExecutorResolver _executorResolver = null!;

    [Params(DbContextKind.Resolver, DbContextKind.Pooled)]
    public DbContextKind DbContextKind { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var servicesCollection = new ServiceCollection();
        ServicesConfiguration.ConfigureServices(servicesCollection, DbContextKind);
        var services = servicesCollection.BuildServiceProvider();
        _executorResolver = services.GetRequiredService<IRequestExecutorResolver>();
    }

    private const string ExtensiveQueryBase = @"{
    users<limit> {
        id
        username
        catchPhrase
        reputation
        totalWealth
        devices {
            id
            name
            price
            goldGramsPrice
        }
    }
}";
    private static readonly string ExtensiveQueryLimit1
        = ExtensiveQueryBase.Replace("<limit>", "(limit: 1)");
    private static readonly string ExtensiveQueryLimit10
        = ExtensiveQueryBase.Replace("<limit>", "(limit: 10)");
    private static readonly string ExtensiveQueryLimit50
        = ExtensiveQueryBase.Replace("<limit>", "(limit: 50)");
    private static readonly string ExtensiveQueryLimit500
        = ExtensiveQueryBase.Replace("<limit>", "(limit: 500)");
    
    [Benchmark]
    [Arguments(1)]
    [Arguments(10)]
    [Arguments(50)]
    [Arguments(500)]
    public Task ExtensiveQuery(int limit)
    {
        var query = limit switch
        {
            1 => ExtensiveQueryLimit1,
            10 => ExtensiveQueryLimit10,
            50 => ExtensiveQueryLimit50,
            500 => ExtensiveQueryLimit500,
            _ => throw new ArgumentException("Invalid limit.", nameof(limit))
        };
        return RunQuery(query);
    }

    private const string IntermediateQueryBase = @"{
    users<limit> {
        id
        username
        catchPhrase
        reputation
        totalWealth
    }
}";
    private static readonly string IntermediateQueryLimit1
        = IntermediateQueryBase.Replace("<limit>", "(limit: 1)");
    private static readonly string IntermediateQueryLimit10
        = IntermediateQueryBase.Replace("<limit>", "(limit: 10)");
    private static readonly string IntermediateQueryLimit50
        = IntermediateQueryBase.Replace("<limit>", "(limit: 50)");
    private static readonly string IntermediateQueryLimit500
        = IntermediateQueryBase.Replace("<limit>", "(limit: 500)");
    
    [Benchmark]
    [Arguments(1)]
    [Arguments(10)]
    [Arguments(50)]
    [Arguments(500)]
    public Task IntermediateQuery(int limit)
    {
        var query = limit switch
        {
            1 => IntermediateQueryLimit1,
            10 => IntermediateQueryLimit10,
            50 => IntermediateQueryLimit50,
            500 => IntermediateQueryLimit500,
            _ => throw new ArgumentException("Invalid limit.", nameof(limit))
        };
        return RunQuery(query);
    }

    private const string SmallQueryBase = @"{
    users<limit> {
        id
        username
        catchPhrase
        reputation
    }
}";
    private static readonly string SmallQueryLimit1
        = SmallQueryBase.Replace("<limit>", "(limit: 1)");
    private static readonly string SmallQueryLimit10
        = SmallQueryBase.Replace("<limit>", "(limit: 10)");
    private static readonly string SmallQueryLimit50
        = SmallQueryBase.Replace("<limit>", "(limit: 50)");
    private static readonly string SmallQueryLimit500
        = SmallQueryBase.Replace("<limit>", "(limit: 500)");
    
    [Benchmark]
    [Arguments(1)]
    [Arguments(10)]
    [Arguments(50)]
    [Arguments(500)]
    public Task SmallQuery(int limit)
    {
        var query = limit switch
        {
            1 => SmallQueryLimit1,
            10 => SmallQueryLimit10,
            50 => SmallQueryLimit50,
            500 => SmallQueryLimit500,
            _ => throw new ArgumentException("Invalid limit.", nameof(limit))
        };
        return RunQuery(query);
    }

    [Benchmark]
    public Task ApplyInflation()
    {
        return RunQuery(@"mutation {
    applyInflation(userId: 1) {
    }
}");
    }
    
    protected async Task RunQuery(string queryText)
    {
        IRequestExecutor executor = await _executorResolver.GetRequestExecutorAsync();
        IExecutionResult result = await executor.ExecuteAsync(new QueryRequest(new QuerySourceText(queryText)));

        if (result is IQueryResult { Errors.Count: > 0 })
        {
            throw new InvalidOperationException("The request failed.");
        }

        if (result is IAsyncDisposable d)
        {
            await d.DisposeAsync();
        }
    }
}

public class HotBenchmarkOrderer : IOrderer 
{
    public IEnumerable<BenchmarkCase> GetExecutionOrder(ImmutableArray<BenchmarkCase> benchmarksCase)
    {
        return benchmarksCase;
    }

    public IEnumerable<BenchmarkCase> GetSummaryOrder(ImmutableArray<BenchmarkCase> benchmarksCases, Summary summary)
    {
        var benchmarkLogicalGroups = benchmarksCases.GroupBy(b => GetLogicalGroupKey(benchmarksCases, b));
        foreach (var logicalGroup in GetLogicalGroupOrder(benchmarkLogicalGroups))
        foreach (var benchmark in logicalGroup)
            yield return benchmark;
    }

    public string GetHighlightGroupKey(BenchmarkCase benchmarkCase) => null;

    public string GetLogicalGroupKey(ImmutableArray<BenchmarkCase> allBenchmarksCases, BenchmarkCase benchmarkCase)
    {
        return benchmarkCase.Job.DisplayInfo + "_" + (benchmarkCase.Parameters["limit"] ?? "none");
    }

    public IEnumerable<IGrouping<string, BenchmarkCase>> GetLogicalGroupOrder(IEnumerable<IGrouping<string, BenchmarkCase>> logicalGroups)
    {
        return logicalGroups.OrderBy(x => x.Key);
    }

    public bool SeparateLogicalGroups => true;
}

public class HotBenchmarkConfig : ManualConfig
{
    public HotBenchmarkConfig()
    {
        Orderer = new HotBenchmarkOrderer();
    }
}