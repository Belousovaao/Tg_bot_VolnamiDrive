
using VolnamiDriveBot.Models.Domain;

namespace VolnamiDriveBot.Services.VehicleService
{
    public interface IVehicleService
    {
        /// <summary>
        /// Выгружает словарь ТС, соответствующих заданному типу
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Dictionary<string, Vehicle> GetVehiclesByType(string type);
        
        /// <summary>
        /// Возвращает ТС по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Vehicle GetVehicle(string id);
        
        /// <summary>
        /// Возвращает словарь всех существующих ТС
        /// </summary>
        /// <returns></returns>
        Dictionary<string, Vehicle> GetAllVehicles();
    }
}
