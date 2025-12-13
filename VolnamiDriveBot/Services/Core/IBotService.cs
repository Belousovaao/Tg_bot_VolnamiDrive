using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolnamiDriveBot.Services.Core
{
    public interface IBotService
    {
        void StartBot(CancellationToken cancellationToken);
    }
}
