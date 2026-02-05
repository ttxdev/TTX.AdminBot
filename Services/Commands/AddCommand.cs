using Discord;
using Discord.WebSocket;

namespace TTX.AdminBot.Services.Commands;

public class AddCommand(TtxApiClient _api) : ICommand
{
    public bool IsAdminOnly => true;
    public SlashCommandBuilder Info => new SlashCommandBuilder()
        .WithName("add")
        .WithDescription("Add a new creator")
        .WithContextTypes([InteractionContextType.Guild])
        .WithDefaultPermission(false)
        .AddOption("slug", ApplicationCommandOptionType.String, "The slug for the creator", true)
        .AddOption("ticker", ApplicationCommandOptionType.String, "The ticker for the creator", true);

    public async Task Execute(SocketSlashCommand ctx)
    {
        string slug = (string)ctx.Data.Options.First().Value;
        string ticker = (string)ctx.Data.Options.Last().Value;
        await ctx.DeferAsync();
        (bool isOk, string resp) = await _api.CreateCreator(slug, ticker);
        if (isOk)
        {
            await ctx.FollowupAsync($"Added. https://ttx.gg/{slug}");
            return;
        }

        await ctx.FollowupAsync(
            $"""
            Error adding creator.
            ```json
            {resp}
            ```
            """
        );
    }
}
