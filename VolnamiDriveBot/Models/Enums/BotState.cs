using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolnamiDriveBot.Models.Enums
{
    public enum BotState
    {
        Default,
        AwaitingWishes,
        AwaitingPassportPhoto,
        AwaitingPassportRegistration,
        AwaitingDrivingLicenseFrontPhoto,
        AwaitingDrivingLicenseBackPhoto,
        AwaitingContactInfo,
        AdminReplyingToUser
    }
}
