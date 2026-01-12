using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolnamiDriveBot.Models.Domain
{
    public class BookingRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
        public string VehicleId { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal PawnPrice { get; set; }
        public string? TelegramUsername { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ContactInfo {  get; set; }

        // Пути к документам
        public string? PassportMainPhotoFileId { get; set; }
        public string? PassportRegistrationPhotoFileId { get; set; }
        public string? DrivingLicenseFrontPhotoFileId { get; set; }
        public string? DrivingLicenseBackPhotoFileId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // методы для связи
        public string GetContactLink()
        {
            if (!string.IsNullOrEmpty(TelegramUsername))
            {
                // Очищаем username от @ если есть
                string cleanUsername = TelegramUsername.TrimStart('@');
                return $"https://t.me/{cleanUsername}";
            }

            if (!string.IsNullOrEmpty(PhoneNumber))
            {
                return $"{PhoneNumber}";
            }

            return null;
        }
    }
}
