using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;
using VolnamiDriveBot.Models.Domain;
using VolnamiDriveBot.Services.Admin;
using VolnamiDriveBot.Services.VehicleService;
using static System.Net.Mime.MediaTypeNames;

namespace VolnamiDriveBot.Services.BookingService
{
    public class BookingService : IBookingService
    {
        private readonly string _dataFile;
        private readonly ILogger<BookingService> _logger;
        private Dictionary<string, VehicleAvailability> _vehicleAvailabilities;
        private readonly IVehicleService _vehicleService;    
        private const int DAYS_AHEAD = 100;
        private readonly Dictionary<string, BookingRequest> _requests = new();

        public BookingService(ILogger<BookingService> logger, IVehicleService vehicleService)
        {
            _logger = logger;
            _vehicleService = vehicleService;
            string projectRoot = FindProjectRoot(AppContext.BaseDirectory);
            _dataFile = Path.Combine(projectRoot, "Data", "booking_data.json");
            LoadData();
            UpdateAvailableDates();
        }
        /// <summary>
        /// Поиск и возврат корневой директории проекта.
        /// </summary>
        /// <param name="startDirectory"></param>
        /// <returns></returns>
        private string FindProjectRoot(string startDirectory)
        {
            DirectoryInfo directory = new DirectoryInfo(startDirectory);
            while (directory != null)
            {
                if (directory.GetFiles("*.csproj").Any())
                    return directory.FullName;
                directory = directory.Parent;
            }
            return Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Загружает данные из базы json файла
        /// </summary>
        private void LoadData()
        {
            try
            {
                if (!File.Exists(_dataFile))
                {
                    _logger.LogWarning("Файл данных не найден");
                    _vehicleAvailabilities = new Dictionary<string, VehicleAvailability>();
                }

                string json = File.ReadAllText(_dataFile);
                if (string.IsNullOrEmpty(json))
                {
                    _logger.LogWarning("❌ Файл данных пустой, создаем новый");
                    _vehicleAvailabilities = new Dictionary<string, VehicleAvailability>();
                    return;
                }

                var data = JsonSerializer.Deserialize<Dictionary<string, VehicleAvailability>>(json);
                _vehicleAvailabilities = data ?? new Dictionary<string, VehicleAvailability>();
                _logger.LogInformation("Данные бронирования загружены");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки данных брони");
                _vehicleAvailabilities = new Dictionary<string, VehicleAvailability>();
            }
        }

        /// <summary>
        /// Создаёт новые даты на 100 дней вперед для каждого ТС, если ранее их не было
        /// </summary>
        private void UpdateAvailableDates()
        {
            DateTime startDate = DateTime.Today;
            bool updated = false;

            Dictionary<string, Vehicle> allVehicles = _vehicleService.GetAllVehicles();

            foreach (string vehicleId in allVehicles.Keys)
            {
                //если был создано новое ТС, ранее не существовавший в базе, для него создаётся пустая строка
                if (!_vehicleAvailabilities.ContainsKey(vehicleId))
                {
                    _vehicleAvailabilities[vehicleId] = new VehicleAvailability
                    {
                        AvailableDates = new Dictionary<string, DateAvailability>()
                    };
                    updated = true;
                }
                VehicleAvailability vehicleData = _vehicleAvailabilities[vehicleId];

                for (int i = 0; i <= DAYS_AHEAD; i++)
                {
                    DateTime currentDate = startDate.AddDays(i);
                    string dateKey = currentDate.ToString("yyyy-MM-dd");

                    if (!vehicleData.AvailableDates.ContainsKey(dateKey))
                    {
                        vehicleData.AvailableDates[dateKey] = new DateAvailability
                        {
                            IsAvailable = true
                        };
                        updated = true;
                    }
                }
            }

            if (updated)
            {
                SaveData();
                _logger.LogInformation("✅ Обновлены доступные даты.");
            }
        }

        public Dictionary<DateTime, bool> GetAvailableDates(string vehicleId, int daysAhead = DAYS_AHEAD)
        {
            Dictionary<DateTime, bool> result = new Dictionary<DateTime, bool>();
            DateTime startDate = DateTime.Today;

            _logger.LogDebug("Получение доступных дат для vehicle {VehicleId}", vehicleId);

            VehicleAvailability vehicleData = _vehicleAvailabilities[vehicleId];

            for (int i = 0; i < daysAhead; i++)
            {
                DateTime currentDate = startDate.AddDays(i);
                string dateKey = currentDate.ToString("yyyy-MM-dd");

                result[currentDate] = vehicleData.AvailableDates[dateKey].IsAvailable;
            }

            return result;
        }

        /// <summary>
        /// Сохраняет новые данные в json файл
        /// </summary>
        private void SaveData()
        {
            try
            {
                var json = JsonSerializer.Serialize(_vehicleAvailabilities, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                File.WriteAllText(_dataFile, json);
                _logger.LogInformation("✅ Данные сохранены. Дат: {DatesCount}", _vehicleAvailabilities.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка сохранения данных брони");
            }
        }

        public async Task<BookingRequest> CreateBookingRequest(long userId, string userName, string vehicleId, string vehicleName, DateTime startDate,DateTime endDate, decimal totalPrice, decimal pawnPrice)
        {
            BookingRequest request = new BookingRequest
            {
                UserId = userId,
                UserName = userName,
                VehicleId = vehicleId,
                VehicleName = vehicleName,
                StartDate = startDate,
                EndDate = endDate,
                TotalDays = (endDate - startDate).Days,
                TotalPrice = totalPrice,
                PawnPrice = pawnPrice
            };

            _requests[request.Id] = request;
            _logger.LogInformation("Создана заявка на бронирование {RequestId} для пользователя {UserId}",
                request.Id, userId);

            return request;
        }

        public async Task<bool> AddPassportMainPhoto(string requestId, string fileId)
        {
            if (_requests.TryGetValue(requestId, out BookingRequest request))
            {
                request.PassportMainPhotoFileId = fileId;
                _logger.LogDebug("Добавлено фото паспорта к заявке {RequestId}", requestId);
                return true;
            }

            _logger.LogWarning("Заявка {RequestId} не найдена при добавлении фото паспорта", requestId);
            return false;
        }

        public async Task<bool> AddPassportRegistrationPhoto(string requestId, string fileId)
        {
            if (_requests.TryGetValue(requestId, out var request))
            {
                request.PassportRegistrationPhotoFileId = fileId;
                _logger.LogDebug("Добавлено фото паспорта (прописка) к заявке {RequestId}", requestId);
                return true;
            }
            return false;
        }

        public async Task<bool> AddDrivingLicenseFrontPhoto(string requestId, string fileId)
        {
            if (_requests.TryGetValue(requestId, out var request))
            {
                request.DrivingLicenseFrontPhotoFileId = fileId;
                _logger.LogDebug("Добавлено фото ВУ к заявке {RequestId}", requestId);
                return true;
            }

            _logger.LogWarning("Заявка {RequestId} не найдена при добавлении фото ВУ", requestId);
            return false;
        }

        public async Task<bool> AddDrivingLicenseBackPhoto(string requestId, string fileId)
        {
            if (_requests.TryGetValue(requestId, out var request))
            {
                request.DrivingLicenseBackPhotoFileId = fileId;
                _logger.LogDebug("Добавлено фото ВУ (оборотная) к заявке {RequestId}", requestId);

                return true;
            }
            return false;
        }

        public Task<BookingRequest?> GetRequest(string requestId)
        {
            _requests.TryGetValue(requestId, out var request);
            return Task.FromResult(request);
        }

        public async Task<bool> AddContactInfo(string requestId, string contactInfo)
        {
            if (!_requests.TryGetValue(requestId, out BookingRequest request))
                return false;
            try
            {
                contactInfo = contactInfo.Trim();

                //ищем userName
                string userName = "";

                string[] patterns = new[]
                {
                    @"@([a-zA-Z0-9_]{5,32})",                     // @username
                    @"t\.me/([a-zA-Z0-9_]{5,32})",                // t.me/username
                    @"https?://t\.me/([a-zA-Z0-9_]{5,32})",       // https://t.me/username
                    @"telegram\s*[:\.]?\s*@?([a-zA-Z0-9_]{5,32})",// telegram: @username
                    @"tg\s*[:\.]?\s*@?([a-zA-Z0-9_]{5,32})",      // tg: username
                    @"([a-zA-Z][a-zA-Z0-9_]{4,31})\b(?!\d)"       // просто username (без цифр в начале)
                };

                foreach (string pattern in patterns)
                {
                    Match match = Regex.Match(contactInfo, pattern, RegexOptions.IgnoreCase);
                    if (match.Success && match.Groups.Count > 1)
                    {
                        userName = match.Groups[1].Value.Trim();
                        break;
                    }
                }

                // убираем из строки поиска userName
                string phoneNumber = contactInfo;

                if (!string.IsNullOrEmpty(userName))
                {
                    string[] patterns2 = new[]
                    {
                        $@"@{userName}\b",
                        $@"t\.me/{userName}\b",
                        $@"https?://t\.me/{userName}\b",
                        $@"\b{userName}\b"
                    };

                    foreach (string pattern in patterns2)
                    {
                        phoneNumber = Regex.Replace(phoneNumber, pattern, "", RegexOptions.IgnoreCase);
                    }

                    request.TelegramUsername = userName;
                }

                phoneNumber = phoneNumber.Trim();


                // Убираем всё лишнее из номера, оставляем только цифры
                if (!string.IsNullOrWhiteSpace(phoneNumber))
                {
                    // Оставляем только цифры и + в начале
                    phoneNumber = Regex.Replace(phoneNumber, @"[^\d+]", "");

                    // Если номер начинается с 8, меняем на +7
                    if (phoneNumber.StartsWith("8") && phoneNumber.Length >= 11)
                    {
                        phoneNumber = "+7" + phoneNumber.Substring(1);
                    }

                    request.PhoneNumber = phoneNumber;
                }
                return true;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка сохранения контактов для заявки {RequestId}", requestId);

                // В случае ошибки сохраняем всё как телефон
                request.PhoneNumber = contactInfo.Trim();
                return true; // Всегда возвращаем true!
            }
        }

            //if (contactInfo.StartsWith("@") || contactInfo.Contains("t.me/"))
            //    {
            //        request.TelegramUsername = contactInfo.Replace("@", "").Replace("t.me/", "").Trim();
            //    }
            //    else
            //    {
            //        request.PhoneNumber = contactInfo;
            //    }

            //    return true;
    }
}
