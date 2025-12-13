using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using VolnamiDriveBot.Commands.Concrete;
using VolnamiDriveBot.Configuration;
using VolnamiDriveBot.Services.BookingService;
using VolnamiDriveBot.Services.Calendar;
using VolnamiDriveBot.Services.Commands;
using VolnamiDriveBot.Services.Core;
using VolnamiDriveBot.Services.Handlers;
using VolnamiDriveBot.Services.Keyboard;
using VolnamiDriveBot.Services.StateManagement;
using VolnamiDriveBot.Services.VehicleService;

internal class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();

        var services = ConfigureServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var botService = serviceProvider.GetRequiredService<IBotService>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        using CancellationTokenSource cts = new CancellationTokenSource();

        try
        {
            botService.StartBot(cts.Token);
            logger.LogInformation("Бот успешно запущен");

            Console.ReadLine();
            cts.Cancel();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Критическая ошибка в боте");
        }    
    }

    private static IServiceCollection ConfigureServices(IConfiguration configuration)
    {
        ServiceCollection services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);

        // Логирование
        services.AddLogging(builder => builder.AddConsole());

        // Конфигурация
        services.Configure<BotConfiguration>(configuration.GetSection("BotConfiguration"));

        // Регистрация Telegram Bot Client
        services.AddSingleton<ITelegramBotClient>(provider =>
        {
            var config = provider.GetRequiredService<IOptions<BotConfiguration>>();
            return new TelegramBotClient(config.Value.Token);
        });

        //Регистрация обработчиков
        services.AddSingleton<MessageHandler>();
        services.AddSingleton <CallbackQueryHandler>();

        // Регистрация сервисов
        services.AddSingleton<IBotService, BotService>();
        services.AddSingleton<IUpdateHandler, TelegramUpdateHandler>();
        services.AddSingleton<IUserStateManager, UserStateManager>();
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<IBookingService, BookingService>();
        services.AddSingleton<ICalendarManager, CalendarManager>();
        services.AddSingleton <IKeyboardService, KeyboardService>();
        services.AddSingleton <IVehicleService, VehicleService>();


        // Регистрация команд
        services.AddTransient<StartCommand>();
        services.AddTransient<HelpCommand>();
        services.AddTransient<ContactCommand>();

        return services;
    }
}