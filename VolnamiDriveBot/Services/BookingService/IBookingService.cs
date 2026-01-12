using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolnamiDriveBot.Models.Domain;

namespace VolnamiDriveBot.Services.BookingService
{
    public interface IBookingService
    {
        /// <summary>
        /// Возвращает словарь дат, для выбранного ТС, где для каждой даты установлено значение доступности в формате true/false
        /// </summary>
        /// <param name="vehicleId"></param>
        /// <param name="daysAhead"></param>
        /// <returns></returns>
        /// 
        Dictionary<DateTime, bool> GetAvailableDates(string vehicleId, int daysAhead = 100);
        /// <summary>
        /// Создаёт экземпляр заявки на бронь
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="vehicleId"></param>
        /// <param name="vehicleName"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="totalPrice"></param>
        /// <param name="pawnPrice"></param>
        /// <returns></returns>
        Task<BookingRequest> CreateBookingRequest(long userId, string userName, string vehicleId, string vehicleName, DateTime startDate, DateTime endDate, decimal totalPrice, decimal pawnPrice);

        /// <summary>
        /// Добавляет файл/фото в поле брони PassportMainPhotoFileId, возвращает true при успехе и false при невозможности
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        Task<bool> AddPassportMainPhoto(string requestId, string fileId);

        /// <summary>
        /// Добавляет файл/фото в поле брони PassportRegistrationPhotoFileId, возвращает true при успехе и false при невозможности
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        Task<bool> AddPassportRegistrationPhoto(string requestId, string fileId);

        /// <summary>
        /// Добавляет файл/фото в поле брони DrivingLicenseFrontPhotoFileId, возвращает true при успехе и false при невозможности
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        Task<bool> AddDrivingLicenseFrontPhoto(string requestId, string fileId);

        /// <summary>
        /// Добавляет файл/фото в поле брони DrivingLicenseBackPhotoFileId, возвращает true при успехе и false при невозможности
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        Task<bool> AddDrivingLicenseBackPhoto(string requestId, string fileId);

        /// <summary>
        /// По id заявки получает доступ к заявке и записывает в её поля номер телефона или имя пользователя в тг
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="contactInfo"></param>
        /// <returns></returns>
        Task<bool> AddContactInfo(string requestId, string contactInfo);

        /// <summary>
        /// Ищет и возвращает объект заявки по его id
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        Task<BookingRequest?> GetRequest(string requestId);
    }
}
