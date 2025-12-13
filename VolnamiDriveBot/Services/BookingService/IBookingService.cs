using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolnamiDriveBot.Services.BookingService
{
    public interface IBookingService
    {
        Dictionary<DateTime, bool> GetAvailableDates(string vehicleId, int daysAhead = 100);
    }
}
