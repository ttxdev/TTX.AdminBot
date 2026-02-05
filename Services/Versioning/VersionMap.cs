using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Discord;
using TTX.AdminBot.Services.Commands;

namespace TTX.AdminBot.Services.Versioning;

public class VersionMap
{
    private readonly Dictionary<string, string> _map;

    public VersionMap()
    {
        _map = [];
    }

    public VersionMap(Dictionary<string, string> map)
    {
        _map = map;
    }

    public VersionMap(IEnumerable<ICommand> commands)
    {
        _map = commands.ToDictionary(
            c => c.Info.Name,
            Hash
        );
    }

    public bool NeedsUpdated(IApplicationCommand command)
    {
        if (!_map.TryGetValue(command.Name, out string? version))
        {
            return true;
        }

        string currentVersion = Hash(command);
        if (version != currentVersion)
        {
            return false;
        }

        return true;
    }

    public bool NeedsUpdated(ICommand command)
    {
        if (!_map.TryGetValue(command.Info.Name, out string? version))
        {
            return true;
        }

        string currentVersion = Hash(command);
        if (version != currentVersion)
        {
            return false;
        }

        return true;
    }

    public Task Save(string file)
    {
        string data = JsonSerializer.Serialize(_map);
        return File.WriteAllTextAsync(file, data);
    }

    public static async Task<VersionMap> Load(string file)
    {
        if (!File.Exists(file))
        {
            return new VersionMap();
        }

        string json = await File.ReadAllTextAsync(file);
        Dictionary<string, string> map = JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
        return new VersionMap(map);
    }

    private static string Hash(ICommand command) => Hash(CreateApplicationCommandParams.Create(command.Info.Build()));
    private static string Hash(IApplicationCommand command) => Hash(CreateApplicationCommandParams.Create(command));
    private static string Hash(CreateApplicationCommandParams @params)
    {
        string data = JsonSerializer.Serialize(@params);
        return BitConverter.ToString(MD5.HashData(Encoding.UTF8.GetBytes(data)));
    }
}
