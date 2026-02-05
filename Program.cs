using TTX.AdminBot;
using TTX.AdminBot.Options;
using TTX.AdminBot.Services;
using TTX.AdminBot.Services.Commands;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Services
    .AddOptions<TtxOptions>()
    .Bind(builder.Configuration.GetSection("Ttx"))
    .Services
    .AddOptions<BotOptions>()
    .Bind(builder.Configuration.GetSection("Bot"))
    .Configure(opt =>
    {
        opt.Migrate = args.FirstOrDefault() == "migrate";
    })
    .Services
    .AddHttpClient()
    .AddLogging(opt =>
    {
        opt.AddConsole();
    })
    .AddSingleton<Bot>()
    .AddSingleton<TtxApiClient>()
    .AddScoped<ICommand, AddCommand>()
    .AddHostedService<Worker>();

IHost host = builder.Build();
host.Run();
