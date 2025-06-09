using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MindBot.Core;
using MindBot.Core.Options;
using MindBot.EF;
using MindBot.EF.Interfaces;
using MindBot.EF.Repositories;
using MindBot.Services.BackgroundServices;
using MindBot.Services.Interfaces;
using MindBot.Services.Services;
using Serilog;
using Serilog.Formatting.Json;
using Telegram.Bot;

var appName = "MindTestBot";

var builder = WebApplication.CreateBuilder(args);

/// Создаем папку для логов
var logPath = Path.Combine(AppContext.BaseDirectory, "Logs");
if (!Directory.Exists(logPath))
{
    Directory.CreateDirectory(logPath);
}

/// Настройка Serilog из конфигурации
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

try
{
    Log.Information($"Starting {appName} application");

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: Path.Combine(logPath, "log-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 21,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                fileSizeLimitBytes: 10_000_000,
                rollOnFileSizeLimit: true,
                shared: true)
            .WriteTo.File(
                formatter: new JsonFormatter(),
                path: Path.Combine(logPath, "log-.json"),
                rollingInterval: RollingInterval.Day);
    });

    ConfigureServices(builder.Services, builder, builder.Configuration);

    var app = builder.Build();
    ConfigureMiddleWare(app, builder);

    app.Run(async (context) =>
    {
        var response = context.Response;
        response.ContentType = "text/html; charset=utf-8";
        await response.WriteAsync($"<h2>{appName}</h2><h3>Приложение запущено</h3>");
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder, IConfiguration configuration)
{
    /// Подключение к БД
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    var dbConncetionString = configuration.GetConnectionString(Settings.DatabaseConnectionName);
    services.AddDbContext<MindBotDbContext>(options => options.UseNpgsql(dbConncetionString), ServiceLifetime.Transient);

    services.AddOptions<TelegramOption>().Configure(options =>
    {
        configuration.GetSection("TelegramBot").Bind(options);
    });

    services
        .AddHttpClient("telegram_bot_client")
        .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
        {
            var options = sp.GetRequiredService<IOptions<TelegramOption>>();
            return new TelegramBotClient(options.Value.Token, httpClient);
        });

    /// Подключение репозитория
    services.AddTransient<IUserStateRepository, UserStateRepository>();

    /// Подключение сервисов чат-бота
    services.AddTransient<IUserStateService, UserStateService>();
    services.AddTransient<IQuestionService, QuestionService>();
    services.AddTransient<IScriptService, ScriptService>();
    services.AddTransient<IBotService, BotService>();

    /// Регистрируем фоновый сервис
    builder.Services.AddHostedService<BotBackgroundService>();

    var strHostValue = builder.Configuration["AppHost:HostValue"];
    builder.WebHost.UseUrls(strHostValue);
}

void ConfigureMiddleWare(WebApplication app, WebApplicationBuilder builder)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<MindBotDbContext>();
        context.Database.Migrate();
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
}

