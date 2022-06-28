using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HotChocolateEFBenchmarks.Data;

public class HotBenchDbContext : DbContext
{
    public const int UserCount = 500;
    public const int DevicesPerUser = 5;

    public static void ConfigureOptions(DbContextOptionsBuilder builder, IConfiguration config)
    {
        builder.UseNpgsql(config["ConnectionString"])
            .UseSnakeCaseNamingConvention();
    }
    
    public HotBenchDbContext(DbContextOptions<HotBenchDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; private set; } = null!;
    public DbSet<Device> Devices { get; private set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>()
            .Property(x => x.Price)
            .HasPrecision(10, 2);
    }
    
    public async Task Seed()
    {
        var faker = new Faker();
        for (int i = 0; i < UserCount; i++)
        {
            var user = new User(
                faker.Internet.UserName(),
                faker.Company.CatchPhrase(),
                faker.Random.Int(0, 99999));
            for (int j = 0; j < DevicesPerUser; j++)
            {
                var device = new Device(user, 
                    faker.Commerce.ProductName(), 
                    faker.Random.Decimal(0.0m, 100_000.0m));
                user.Devices.Add(device);
            }

            Users.Add(user);
        }

        await SaveChangesAsync();
    }

    public async Task Nuke()
    {
        var allDevices = await Devices.ToListAsync();
        var allUsers = await Users.ToListAsync();
        
        Devices.RemoveRange(allDevices);
        Users.RemoveRange(allUsers);

        await SaveChangesAsync();
    }

    public async Task Reset()
    {
        await Nuke();
        await Seed();
    }
}

public class HotBenchDesignTimeDbContextFactory : IDesignTimeDbContextFactory<HotBenchDbContext>
{
    public HotBenchDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<HotBenchDbContext>();
        var config = ServicesConfiguration.BuildConfiguration();
        HotBenchDbContext.ConfigureOptions(builder, config);
        
        return new HotBenchDbContext(builder.Options);
    }
}