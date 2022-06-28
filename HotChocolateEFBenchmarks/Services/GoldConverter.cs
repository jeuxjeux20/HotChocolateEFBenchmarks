namespace HotChocolateEFBenchmarks.Services;

public class GoldConverter
{
    public decimal ConvertDollarsToGoldGrams(decimal value)
    {
        return value / 48.052m;
    }
}