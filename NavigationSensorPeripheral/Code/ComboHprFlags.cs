using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PartnerCatalyst.NavigationSensorPeripheral
{
    [Flags]
    public enum ComboHprFlags : byte
    {
        StepDetected = 0x01,
        MageneticDesturbance = 0x02,
        AccelerometerIndicatesMovement = 0x04,
        GyroindIcatesMovement = 0x08, 
        AutoCalibrated = 0x10
    }
}
