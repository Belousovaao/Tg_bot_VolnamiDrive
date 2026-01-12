using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using VolnamiDriveBot.Commands;
using VolnamiDriveBot.Models.Domain;
using VolnamiDriveBot.Models.Enums;
using VolnamiDriveBot.Services.BookingService;
using VolnamiDriveBot.Services.Calendar;
using VolnamiDriveBot.Services.Commands;
using VolnamiDriveBot.Services.Keyboard;
using VolnamiDriveBot.Services.StateManagement;
using VolnamiDriveBot.Services.VehicleService;

namespace VolnamiDriveBot.Services.Handlers
{
    public class CallbackQueryHandler
    {
        private readonly IUserStateManager _stateManager;
        private readonly ICommandFactory _commandFactory;
        private readonly ILogger<CallbackQueryHandler> _logger;
        private readonly IKeyboardService _keyboardService;
        private readonly IVehicleService _vehicleService;
        private readonly ICalendarManager _calendarManager;
        private readonly IBookingService _bookingService;
        private readonly AdminCallbackHandler _adminCallbackHandler;

        public CallbackQueryHandler(IUserStateManager stateManager, ICommandFactory commandFactory, ILogger<CallbackQueryHandler> logger, IKeyboardService keyboardService, IVehicleService vehicleService, ICalendarManager calendarManager, IBookingService bookingService, AdminCallbackHandler adminCallbackHandler)
        {
            _stateManager = stateManager;
            _commandFactory = commandFactory;
            _logger = logger;
            _keyboardService = keyboardService;
            _vehicleService = vehicleService;
            _calendarManager = calendarManager;
            _bookingService = bookingService;
            _adminCallbackHandler = adminCallbackHandler;
        }


        /// <summary>
        /// Обработчик кликов на кнопки в боте
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="callbackQuery"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken ct)
        {
            try
            {
                long userId = callbackQuery.From.Id;
                UserState userState = _stateManager.GetUserState(userId);
                string? callbackData = callbackQuery.Data;

                if (string.IsNullOrEmpty(callbackData))
                    return;

                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);

                BotSession session = SessionManager.GetORCreateSession(userId);

                // Обработка разных типов callback данных
                string text = string.Empty;
                InlineKeyboardMarkup markup = new(Array.Empty<InlineKeyboardButton>());

                if (callbackData.StartsWith("cal_"))
                {
                    await HandleCalendarAction(botClient, callbackQuery, session, ct);
                    return;
                }
                
                if (callbackData.StartsWith("admin_"))
                {
                    await _adminCallbackHandler.HandleCallbackAsync(botClient, callbackQuery, ct);
                }

                switch (callbackData)
                {
                    case "auto":
                        text = "<b>Выбери машину, которая тебе нравится, и я рассчитаю стоимость.</b>\n\nМой автопарк будет пополняться, поэтому в самом низу есть кнопка, где ты можешь написать, на какой машине тебе хотелось бы прокатиться в следующий раз.";
                        markup = _keyboardService.GenerateVehiclesMenu("auto");
                        break;
                    case "moto":
                        text = "<b>Выбери мотоцикл, который тебе нравится, и я рассчитаю стоимость.</b>\n\nМой мотопарк будет пополняться, поэтому в самом низу есть кнопка, где ты можешь написать, на каком мотоцикле тебе хотелось бы прокатиться в следующий раз.";
                        markup = _keyboardService.GenerateVehiclesMenu("moto");
                        break;

                    case "wishes":
                        text = "<b>Отлично! Хочу узнать твои пожелания.</b>\n\nОтправь следующим сообщением, на чем ты бы хотел прокатиться в следующий раз.";
                        _stateManager.SetUserState(userId, BotState.AwaitingWishes);
                        break;

                    case "price":
                        await ShowCalendarForStartDate(botClient, callbackQuery, session, DateTime.Now.Year, DateTime.Now.Month, ct);
                        return;

                    case "more":
                        Vehicle selectedVehicle = session.GetSessionData<Vehicle>("SelectedVehicle");
                        if (selectedVehicle != null)
                        {
                            text = selectedVehicle.FullDescription;
                            markup = _keyboardService.GenerateMoreMenu();
                        }
                        break;

                    case "go_back":
                        ICommand startCommand = _commandFactory.CreateCommand("go_back", userState.CurrentState);
                        if (startCommand != null)
                        {
                            await startCommand.Execute(callbackQuery.Message, botClient);
                        }
                        return;

                    case "go_booking":
                        await HandleBooking(botClient, callbackQuery, ct);
                        break;


                    default:
                        Vehicle vehicle = _vehicleService.GetVehicle(callbackData);
                        if (vehicle != null)
                        {
                            session.SetSessionData("SelectedVehicleId", vehicle.Id);
                            session.SetSessionData("SelectedVehicleType", vehicle.Type);
                            session.SetSessionData("SelectedVehicle", vehicle);

                            await ShowVehicleMenu(userId, vehicle.PhotoUrl);
                            return;
                        }
                        break;
                }
                if (!string.IsNullOrEmpty(text))
                {
                    await botClient.SendMessage(callbackQuery.Message!.Chat.Id, text, ParseMode.Html, replyMarkup: markup);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки callback query");
            }

            async Task ShowVehicleMenu(long userId, string photoUrl)
            {
                await botClient.SendPhoto(
                   userId,
                   InputFile.FromUri(photoUrl),
                   "<b>Отличный выбор!</b>\nЖелаешь получить больше информации о транспорте или сразу перейдем к выбору дат и рассчету стоимости?",
                   ParseMode.Html,
                   replyMarkup: _keyboardService.GenerateVehicalOptionsMenu()
               );
            }

            async Task HandleCalendarAction(ITelegramBotClient botClient, CallbackQuery callbackQuery, BotSession session, CancellationToken ct)
            {
                long id = callbackQuery.From.Id;
                string? data = callbackQuery.Data;
                string vehicleId = session.GetSessionData<string>("SelectedVehicleId");

                if (data.Contains("cal_reset"))
                {
                    session.SetSessionData("RentalStartDate", DateTime.MinValue);
                    await botClient.SendMessage(id, "🔄 Выбор сброшен");
                    await ShowCalendarForStartDate(botClient, callbackQuery, session, DateTime.Now.Year, DateTime.Now.Month, ct);
                    return;
                }
                else if (data.StartsWith("cal_today"))
                {
                   ;
                    if (session.GetSessionData<DateTime>("RentalStartDate") == DateTime.MinValue)
                    {
                        DateTime startDate = DateTime.Today;
                        session.SetSessionData("RentalStartDate", startDate);
                        int year = startDate.Year;
                        int month = startDate.Month;
                        await ShowCalendarForEndDate(botClient, callbackQuery, session, year, month, startDate, ct);
                    }
                    else
                    {
                        DateTime endDate = DateTime.Today;
                        DateTime startDate = session.GetSessionData<DateTime>("RentalStartDate");
                        await ConfirmRental(botClient, callbackQuery, session, startDate, endDate, ct);
                    }
                }
                else if (data.StartsWith("cal_start_"))
                {
                    // Пользователь выбрал дату начала 
                    string dateStr = data.Replace("cal_start_", "");
                    if (DateTime.TryParse(dateStr, out DateTime startDate))
                    {
                        session.SetSessionData("RentalStartDate", startDate);
                        int year = startDate.Year;
                        int month = startDate.Month;
                        await ShowCalendarForEndDate(botClient, callbackQuery, session, year, month, startDate, ct);
                    }
                }
                else if (data.StartsWith("cal_end_"))
                {
                    // Пользователь выбрал дату окончания
                    var dateStr = data.Replace("cal_end_", "");
                    if (DateTime.TryParse(dateStr, out var endDate))
                    {
                        DateTime startDate = session.GetSessionData<DateTime>("RentalStartDate");
                        session.SetSessionData("RentalEndDate", endDate);
                        await ConfirmRental(botClient, callbackQuery, session, startDate, endDate, ct);
                    }
                }
                else if (data.StartsWith("cal_prev_") || data.StartsWith("cal_next_"))
                {
                    // Навигация по месяцам
                    var parts = data.Replace("cal_prev_", "").Replace("cal_next_", "").Split('_');
                    if (parts.Length >= 2 && int.TryParse(parts[0], out var year) && int.TryParse(parts[1], out var month))
                    {
                        DateTime startDate = DateTime.MinValue;
                        if (parts.Length >= 3 && parts[2] != "null" && DateTime.TryParse(parts[2], out var parsedStartDate))
                        {
                            startDate = parsedStartDate;
                        }
                        if (data.StartsWith("cal_prev_"))
                        {
                            month--;
                            if (month < 1)
                            {
                                month = 12;
                                year--;
                            }
                        }
                        else
                        {
                            month++;
                            if (month > 12)
                            {
                                month = 1;
                                year++;
                            }
                        }

                        if (startDate != DateTime.MinValue)
                        {
                            await ShowCalendarForEndDate(botClient, callbackQuery, session, year, month, startDate, ct);
                        }
                        else
                        {
                            await ShowCalendarForStartDate(botClient, callbackQuery, session, year, month, ct);
                        }
                    }
                    else if (data == "cal_before_start")
                    {
                        await botClient.AnswerCallbackQuery(callbackQuery.Id, "❌ Дата окончания не может быть раньше даты начала!", showAlert: true);
                    }
                    else if (data == "cal_unavailable_range")
                    {
                        await botClient.AnswerCallbackQuery(callbackQuery.Id, "❌ В выбранном периоде есть занятые даты", showAlert: true);
                    }
                }
            }

            async Task ShowCalendarForStartDate(ITelegramBotClient botClient, CallbackQuery callbackQuery, BotSession session, int year, int month, CancellationToken ct)
            {
                string vehicleId = session.GetSessionData<string>("SelectedVehicleId");
                Dictionary<DateTime, bool> availableDates = _bookingService.GetAvailableDates(vehicleId, 100);
                var calendarMarkup = _calendarManager.CreateCalendar(year, month, availableDates);

                await botClient.SendMessage(callbackQuery.Message!.Chat.Id,
                    "🗓️ <b>ВЫБЕРИТЕ ДАТУ НАЧАЛА АРЕНДЫ</b>\n\n" +
                    "✅ <b>Зеленые даты</b> - доступны для бронирования\n" +
                    "🎯 <b>Дата с мишенью</b> - сегодня\n" +
                    "❌ <b>Красные даты</b> - Не доступны для бронирования\n\n" +
                    "💡 <b>Можно арендовать от 1 суток</b>",
                ParseMode.Html,
                replyMarkup: calendarMarkup,
                cancellationToken: ct);
            }

            async Task ShowCalendarForEndDate(ITelegramBotClient botClient, CallbackQuery callbackQuery, BotSession session, int year, int month, DateTime startDate, CancellationToken ct)
            {
                string vehicleId = session.GetSessionData<string>("SelectedVehicleId");
                Dictionary<DateTime, bool> availableDates = _bookingService.GetAvailableDates(vehicleId, 100);
                var calendarMarkup = _calendarManager.CreateCalendar(year, month, availableDates, startDate.AddDays(1));

                await botClient.SendMessage(callbackQuery.Message!.Chat.Id,
                    $"🗓️ <b>ВЫБЕРИТЕ ДАТУ ОКОНЧАНИЯ АРЕНДЫ</b>\n\n" +
                    $"📅 <b>Начало:</b> {startDate:dd MMMM yyyy}\n\n" +
                    "✅ <b>Зеленые даты</b> - доступны для завершения аренды\n" +
                    "❌ <b>Красные даты</b> - заняты или недоступны\n\n" +
                    "💡 <b>Минимальный срок аренды - 1 сутки</b>",
                ParseMode.Html,
                replyMarkup: calendarMarkup,
                cancellationToken: ct);
            }

            async Task ConfirmRental(ITelegramBotClient botClient, CallbackQuery callbackQuery, BotSession session, DateTime startDate, DateTime endDate, CancellationToken ct)
            {
                string vehicleId = session.GetSessionData<string>("SelectedVehicleId");
                Vehicle vehicle = _vehicleService.GetVehicle(vehicleId);
                int days = (endDate - startDate).Days ;
                int totalPrice = days * vehicle.DailyPrice;

                var durationText = days == 1 ? "1 сутки" : $"{days} суток";

                await botClient.SendMessage(callbackQuery.Message!.Chat.Id,
                    $"🎉 <b>Рассчет стоимости:</b>\n\n" +
                    $"🚗 ТС: {vehicle.Name}\n" +
                    $"📅 <b>Период аренды:</b>\n" +
                    $"🟢 Начало: {startDate:dd MMMM yyyy}\n" +
                    $"🔴 Окончание: {endDate:dd MMMM yyyy}\n" +
                    $"⏱️ <b>Продолжительность:</b> {durationText}\n" +
                    $"💎 <b>Стоимость:</b> {totalPrice}₽\n\n" +
                    $"Залог: {vehicle.PawnPrice}₽",
                    ParseMode.Html,
                    replyMarkup: _keyboardService.GenerateAfterPriceMenu());
            }

            async Task HandleBooking(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken ct)
            {
                long userId = callbackQuery.From.Id;
                BotSession session = SessionManager.GetORCreateSession(userId);
                string vehicleId = session.GetSessionData<string>("SelectedVehicleId");
                Vehicle vehicle = _vehicleService.GetVehicle(vehicleId);
                DateTime startDate = session.GetSessionData<DateTime>("RentalStartDate");
                DateTime endDate = session.GetSessionData<DateTime>("RentalEndDate");

                // Сохраняем данные для заявки в сессии
                session.SetSessionData("BookingVehicle", vehicle);
                session.SetSessionData("BookingStartDate", startDate);
                session.SetSessionData("BookingEndDate", endDate);

                string userName = $"{callbackQuery.From.FirstName} {callbackQuery.From.LastName}".Trim();
                int days = (endDate - startDate).Days;
                int totalPrice = days * vehicle.DailyPrice;

                //Создаем предварительную заявку
                BookingRequest bookingRequest = await _bookingService.CreateBookingRequest(userId, userName,
                    vehicle.Id, vehicle.Name, startDate, endDate, totalPrice, vehicle.PawnPrice);

                // Сохраняем ID заявки в сессии
                session.SetSessionData("CurrentBookingRequestId", bookingRequest.Id);

                // Просим отправить фото паспорта
                await botClient.SendMessage(
                    callbackQuery.Message.Chat.Id,
                    "📝 <b>Для составления договора аренды мне потребуются ваши данные</b>\n\n" +
                    "Отправьте, пожалуйста, фото/скан вашего паспорта:\n" +
                    "• Разворот с фото (страницы 2-3)\n\n\n" +
                    "🔒 <b>Важная информация о конфиденциальности</b>\n\n" +
                    "Мы серьезно относимся к защите ваших персональных данных:\n\n" +
                    "✅ <b>Данные используются только для:</b>\n" +
                    "• Проверки вашей личности\n" +
                    "• Составления договора аренды\n\n" +
                    "❌ <b>Данные НЕ используются для:</b>\n" +
                    "• Передачи третьим лицам\n" +
                    "• Рассылок или рекламы\n" +
                    "• Других целей без вашего согласия\n\n" +
                    "📝 <b>Все в соответствии с 152-ФЗ «О персональных данных»</b>",
                    ParseMode.Html,
                    cancellationToken: ct);

                _stateManager.SetUserState(userId, BotState.AwaitingPassportPhoto);
            }


        }
    }
}
