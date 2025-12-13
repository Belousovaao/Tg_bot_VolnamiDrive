
using VolnamiDriveBot.Models.Domain;

namespace VolnamiDriveBot.Services.VehicleService
{
    public interface IVehicleService
    {
        Dictionary<string, Vehicle> GetVehiclesByType(string type);
        Vehicle GetVehicle(string id);
        Dictionary<string, Vehicle> GetAllVehicles();
    }
}
