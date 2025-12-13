using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolnamiDriveBot.Models.Domain
{
    public class VehicleAvailability
    {
        public Dictionary<string, DateAvailability> AvailableDates { get; set; } = new();
    }
}
