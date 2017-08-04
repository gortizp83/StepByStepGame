using System;

namespace Microsoft.PartnerCatalyst.NavigationSensorPeripheral
{
    public class GyroChangedEventArgs
    {
        public double Pitch { get; }
        public double Roll { get; }
        public double Yaw { get; }
        public double Compass { get; }

        public DateTimeOffset TimeOffset { get; }

        public ComboHprFlags Flags { get; }

        public GyroChangedEventArgs(double yaw, double pitch, double roll, double compass, ComboHprFlags flags, DateTimeOffset timeOffset)
        {
            this.Flags = flags;
            Pitch = pitch;
            Roll = roll;
            Yaw = yaw;
            Compass = compass;
            TimeOffset = timeOffset; 
        }
    }
}