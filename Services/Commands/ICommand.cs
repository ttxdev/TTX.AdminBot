using Discord;
using Discord.WebSocket;

namespace TTX.AdminBot.Services.Commands;

public interface ICommand
{
    SlashCommandBuilder Info { get; }
    bool IsAdminOnly { get; }
    Task Execute(SocketSlashCommand ctx);
}
