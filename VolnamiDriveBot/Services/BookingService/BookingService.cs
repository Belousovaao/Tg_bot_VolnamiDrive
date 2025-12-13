using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VolnamiDriveBot.Models.Domain;
using VolnamiDriveBot.Services.VehicleService;

namespace VolnamiDriveBot.Services.BookingService
{
    public class BookingService : IBookingService
    {
        private readonly string _dataFile;
        private readonly ILogger<BookingService> _logger;
        private Dictionary<string, VehicleAvailability> _vehicleAvailabilities;
        private readonly IVehicleService _vehicleService;    
        private const int DAYS_AHEAD = 100;

        public BookingService(ILogger<BookingService> logger, IVehicleService vehicleService)
        {
            _logger = logger;
            _vehicleService = vehicleService;
            var projectRoot = FindProjectRoot(AppContext.BaseDirectory);
            _dataFile = Path.Combine(projectRoot, "Data", "booking_data.json");

            _logger.LogInformation("Путь к файлу данных: {DataFile}", _dataFile);
            LoadData();
            UpdateAvailableDates();
        }

        private string FindProjectRoot(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory != null)
        {
            if (directory.GetFiles("*.csproj").Any())
                return directory.FullName;
            directory = directory.Parent;
        }
        return Directory.GetCurrentDirectory();
    }

        private void LoadData()
        {
            if (File.Exists(_dataFile))
            {
                try
                {
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
            else
            {
                _logger.LogWarning("Файл данных не найден");
                _vehicleAvailabilities = new Dictionary<string, VehicleAvailability>();
            }
        }

        private void UpdateAvailableDates()
        {
            DateTime startDate = DateTime.Today;
            bool updated = false;

            Dictionary<string, Vehicle> allVehicles = _vehicleService.GetAllVehicles();

            foreach (string vehicleId in allVehicles.Keys)
            {
                if (!_vehicleAvailabilities.ContainsKey(vehicleId))
                {
                    _vehicleAvailabilities[vehicleId] = new VehicleAvailability
                    {
                        AvailableDates = new Dictionary<string, DateAvailability>()
                    };
                    updated = true;
                }
                var vehicleData = _vehicleAvailabilities[vehicleId];

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

            if (!_vehicleAvailabilities.ContainsKey(vehicleId))
            {
                for (int i = 0; i < daysAhead; i++)
                {
                    result[startDate.AddDays(i)] = true;
                }
                return result;
            }

            var vehicleData = _vehicleAvailabilities[vehicleId];

            for (int i = 0; i < daysAhead; i++)
            {
                DateTime currentDate = startDate.AddDays(i);
                string dateKey = currentDate.ToString("yyyy-MM-dd");

                bool isAvailable = vehicleData.AvailableDates.ContainsKey(dateKey)
                ? vehicleData.AvailableDates[dateKey].IsAvailable
                : true;

                result[currentDate] = isAvailable;
            }

            return result;
        }

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
    }
}
