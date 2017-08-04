using System;

namespace Microsoft.PartnerCatalyst.IntelligentHeadset
{
    [Flags]
    public enum Sensors
    {
        None = 0x0,
        Magnetometer = 0x1,
        Accelerometer = 0x2,
        Gyro = 0x4,
        Gps = 0x8
    };

    public class AuthenticationResult
    {
        public bool Success
        {
            get;
            set;
        }

        public Exception Exception
        {
            get;
            set;
        }
    }

    public class SensorReadingResult
    {
        public bool Success
        {
            get;
            set;
        }

        public Exception Exception
        {
            get;
            set;
        }

        public Sensors SensorsFailed
        {
            get;
            set;
        }
    }
}
