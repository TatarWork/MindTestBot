using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MindTestBot;
using MindTestBot.Interfaces;
using MindTestBot.Models;
using MindTestBot.Services;
using Serilog;
using Serilog.Formatting.Json;
using Telegram.Bot;

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
    Log.Information($"Starting {nameof(MindTestBot)} application");

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
    await ConfigureMiddleWare(app, builder);

    app.Run(async (context) =>
    {
        var response = context.Response;
        response.ContentType = "text/html; charset=utf-8";
        await response.WriteAsync($"<h2>{nameof(MindTestBot)}</h2><h3>Приложение запущено</h3>");
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

    var dbConncetionString = configuration.GetConnectionString(SettingModel.DatabaseConnectionName);
    services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dbConncetionString), ServiceLifetime.Transient);

    services.AddOptions<OptionModel>().Configure(options =>
    {
        configuration.GetSection("TelegramBot").Bind(options);
    });

    services
        .AddHttpClient("telegram_bot_client")
        .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
        {
            var options = sp.GetRequiredService<IOptions<OptionModel>>();
            return new TelegramBotClient(options.Value.Token, httpClient);
        });

    services.AddMemoryCache();

    services.AddTransient<IScriptService, ScriptService>();
    services.AddTransient<IBotService, BotService>();
    
    builder.Services.AddScoped<TestHandler>();

    /// Регистрируем фоновый сервис
    builder.Services.AddHostedService<BotBackgroundService>();
}

async Task ConfigureMiddleWare(WebApplication app, WebApplicationBuilder builder)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();

        var testService = EntrepreneurTestService.Instance;
        await testService.InitializeQuestionsAsync(context);
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
