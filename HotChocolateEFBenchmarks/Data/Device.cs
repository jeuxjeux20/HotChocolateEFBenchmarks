using HotChocolateEFBenchmarks.Services;

namespace HotChocolateEFBenchmarks.Data;

public class Device
{
    public Device(User user, string name, decimal price)
    {
        User = user;
        Name = name;
        Price = price;
    }

    public Device()
    {
        Name = null!;
    }

    public int Id { get; set; }
    public User User { get; set; } = null!;
    public int UserId { get; set; }
    public string Name { get; set; }
    [IsProjected(true)]
    public decimal Price { get; set; } // In dollars
    
    public decimal GetGoldGramsPrice(GoldConverter converter)
    {
        return converter.ConvertDollarsToGoldGrams(Price);
    }
}