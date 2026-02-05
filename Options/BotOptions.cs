namespace TTX.AdminBot.Options;

public class BotOptions
{
    public required string Token { get; init; }
    public required ulong RoleId { get; init; }
    public ulong? TestGuildId { get; init; } = null;
    public bool Migrate = false;
}
