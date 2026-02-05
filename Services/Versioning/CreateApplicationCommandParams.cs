using Discord;

namespace TTX.AdminBot.Services.Versioning;

internal struct CreateApplicationCommandParams
{
    public string Name { get; set; }
    public ApplicationCommandType Type { get; set; }
    public string Description { get; set; }
    public IApplicationCommandOption[] Options { get; set; }
    public bool DefaultPermission { get; set; }
    public IReadOnlyDictionary<string, string> NameLocalizations { get; set; }
    public IReadOnlyDictionary<string, string> DescriptionLocalizations { get; set; }
    public bool Nsfw { get; set; }
    public IReadOnlyCollection<InteractionContextType> ContextTypes { get; set; }
    public IReadOnlyCollection<ApplicationIntegrationType> IntegrationTypes { get; set; }

    public static CreateApplicationCommandParams Create(IApplicationCommand arg)
    {
        return new CreateApplicationCommandParams
        {
            Name = arg.Name,
            Type = arg.Type,
            DefaultPermission = arg.IsDefaultPermission,
            NameLocalizations = arg.NameLocalizations,
            DescriptionLocalizations = arg.DescriptionLocalizations,
            Nsfw = arg.IsNsfw,
            IntegrationTypes = arg.IntegrationTypes,
            ContextTypes = arg.ContextTypes
        };
    }

    public static CreateApplicationCommandParams Create(SlashCommandProperties arg)
    {
        return new CreateApplicationCommandParams
        {
            Name = arg.Name.Value,
            Type = ApplicationCommandType.Slash,
            DefaultPermission = arg.IsDefaultPermission.IsSpecified
                                    ? arg.IsDefaultPermission.Value
                                    : false,
            NameLocalizations = arg.NameLocalizations?.ToDictionary(x => x.Key, y => y.Value) ?? [],
            DescriptionLocalizations = arg.DescriptionLocalizations?.ToDictionary(x => x.Key, y => y.Value) ?? [],

            // TODO: better conversion to nullable optionals
            Nsfw = arg.IsNsfw.GetValueOrDefault(false),
            IntegrationTypes = arg.IntegrationTypes.GetValueOrDefault([]),
            ContextTypes = arg.ContextTypes.GetValueOrDefault([])
        };
    }
}
