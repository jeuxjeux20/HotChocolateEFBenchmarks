using HotChocolate.Execution;
using HotChocolateEFBenchmarks.Data;
using HotChocolateEFBenchmarks.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HotChocolateEFBenchmarks;

public class ServicesConfiguration
{
    public static void ConfigureServices(IServiceCollection collection, DbContextKind contextKind)
    {
        var config = BuildConfiguration();

        void ConfigureDbContext(DbContextOptionsBuilder builder) => HotBenchDbContext.ConfigureOptions(builder, config);
        
        if (contextKind == DbContextKind.Pooled)
        {
            collection.AddPooledDbContextFactory<HotBenchDbContext>(ConfigureDbContext);
            collection.AddTransient<IInflationApplier, FactoryInflationApplier>();
            collection.AddTransient<ITotalWealthDataLoader, FactoryTotalWealthDataLoader>();
        }
        else
        {
            collection.AddDbContextPool<HotBenchDbContext>(ConfigureDbContext);
            collection.AddScoped<IInflationApplier, ScopedInflationApplier>();
            collection.AddScoped<ITotalWealthDataLoader, ScopedTotalWealthDataLoader>();
        }
        
        collection.AddScoped<GoldConverter>(); // Scoped for both as it would be unfair

        collection
            .AddGraphQL()
            .RegisterService<IInflationApplier>()
            .RegisterService<GoldConverter>()
            .RegisterDbContext<HotBenchDbContext>(contextKind)
            .AddQueryType<Query>()
            .AddMutationType<Mutations>()
            .AddProjections();
    }

    public static IConfiguration BuildConfiguration()
    {
       return new ConfigurationBuilder()
            .AddUserSecrets<ServicesConfiguration>()
            .AddEnvironmentVariables()
            .Build();
    }
}