using HotChocolateEFBenchmarks.Services;

namespace HotChocolateEFBenchmarks.Data;

public class User
{
    public User(string username, string catchPhrase, int reputation)
    {
        Username = username;
        CatchPhrase = catchPhrase;
        Reputation = reputation;
    }

    protected User()
    {
        Username = null!;
        CatchPhrase = null!;
    }

    [IsProjected(true)]
    public int Id { get; set; }
    public string Username { get; set; }
    public string CatchPhrase { get; set; }
    public int Reputation { get; set; } = 0;

    public ICollection<Device> Devices { get; set; } = new HashSet<Device>();

    public Task<decimal> GetTotalWealth(ITotalWealthDataLoader loader)
    {
        return loader.LoadAsync(Id);
    }
}