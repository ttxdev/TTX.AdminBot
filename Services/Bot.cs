using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using TTX.AdminBot.Options;
using TTX.AdminBot.Services.Commands;
using TTX.AdminBot.Services.Versioning;

namespace TTX.AdminBot.Services;

public class Bot
{
    private static readonly string MIGRATIONS = AppDomain.CurrentDomain.BaseDirectory + "/Resources/command_migrations.json";
    private readonly ILogger<Bot> _logger;
    private readonly IServiceScopeFactory _scopes;
    private readonly IOptions<BotOptions> _options;
    private readonly DiscordSocketClient _client = new(
        new()
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers
        }
    );

    public Bot(ILogger<Bot> logger, IServiceScopeFactory scopes, IOptions<BotOptions> options)
    {
        _logger = logger;
        _scopes = scopes;
        _options = options;
        _client.Ready += OnReady;
        _client.SlashCommandExecuted += OnSlashCommandExecuted;
    }

    private async Task OnReady()
    {
        if (await ShouldMigrate())
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Migrating to {dest}", _options.Value.TestGuildId.HasValue ?_options.Value.TestGuildId.Value : "global");
            }
            await Migrate();
            _logger.LogInformation("Migration complete.");
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Ready as {bot}", _client.CurrentUser.Username);
        }
    }

    public async Task Start()
    {
        await _client.LoginAsync(TokenType.Bot, _options.Value.Token);
        await _client.StartAsync();
    }

    private async Task<bool> ShouldMigrate()
    {
        if (_options.Value.Migrate)
        {
            return true;
        }

        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
        IReadOnlyCollection<SocketApplicationCommand> registered;
        SocketGuild? testGuild = null;
        if (_options.Value.TestGuildId.HasValue)
        {
            testGuild = _client.GetGuild(_options.Value.TestGuildId.Value);
            registered = await testGuild.GetApplicationCommandsAsync();
        }
        else
        {
            registered = await _client.GetGlobalApplicationCommandsAsync();
        }

        IEnumerable<ICommand> staged = scope.ServiceProvider.GetServices<ICommand>();
        VersionMap map = await VersionMap.Load(MIGRATIONS);
        bool diff = registered
            .Any(command =>
            {
                ICommand? botCommand = staged.FirstOrDefault(c => c.Info.Name == command.Name);
                if (botCommand is null)
                {
                    return true;
                }

                // TODO: fix
                return map.NeedsUpdated(command);
            });

        return diff || registered.Count != registered.Count;
    }

    private async Task Migrate()
    {
        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
        IEnumerable<ICommand> staged = scope.ServiceProvider.GetServices<ICommand>();
        SocketGuild? testGuild = null;
        if (_options.Value.TestGuildId.HasValue)
        {
            testGuild = _client.GetGuild(_options.Value.TestGuildId.Value);
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("{count} commands registered.", staged.Count());
        }

        VersionMap map = new(staged);
        await map.Save(MIGRATIONS);

        ApplicationCommandProperties[] props = [.. staged.Select(s => s.Info.Build())];
        if (testGuild is not null)
        {
            await testGuild.BulkOverwriteApplicationCommandAsync(props);
        }
        else
        {
            await _client.BulkOverwriteGlobalApplicationCommandsAsync(props);
        }
    }

    private async Task OnSlashCommandExecuted(SocketSlashCommand ctx)
    {
        if (!ctx.GuildId.HasValue)
        {
            return;
        }

        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
        ICommand? command = scope.ServiceProvider.GetServices<ICommand>()
            .FirstOrDefault(c => c.Info.Name == ctx.CommandName);
        if (command is null)
        {
            await ctx.RespondAsync("Command not found.", ephemeral: true);
            return;
        }

        if (command.IsAdminOnly && !IsAdmin(ctx))
        {
            await ctx.RespondAsync("Admin only command.", ephemeral: true);
            return;
        }

        await command.Execute(ctx);
    }

    private bool IsAdmin(SocketSlashCommand ctx)
    {
        SocketGuild guild = _client.GetGuild(ctx.GuildId!.Value);
        SocketGuildUser user = guild.GetUser(ctx.User.Id);
        return user.Roles.Any(r => r.Id == _options.Value.RoleId);
    }
}
