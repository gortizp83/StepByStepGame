using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PartnerCatalyst.NavigationSensorPeripheral
{
    //CurrentIntelligentHeadsetReading

    public class CurrentIntelligentHeadsetReading
    {
        public CurrentIntelligentHeadsetReading()
        {
        }

        private ComboHprReading _previousComboHprReading;
        public ComboHprReading PreviousComboHprReading
        {
            get { return _previousComboHprReading; }
        }
        private ComboHprReading _currentComboHprReading;
        public ComboHprReading CurrentComboHprReading
        {
            get { return _currentComboHprReading; }
            set
            {
                _previousComboHprReading = _currentComboHprReading;
                _currentComboHprReading = value;
            }
        }

        private AccelerometerReading _previousAccelerometerReading;
        public AccelerometerReading PreviousAccelerometerReading { get { return _previousAccelerometerReading; } }
        private AccelerometerReading _currentAccelerometerReading;
        public AccelerometerReading CurrentAccelerometerReading
        {
            get { return _currentAccelerometerReading; }
            set
            {
                _previousAccelerometerReading = _currentAccelerometerReading;
                _currentAccelerometerReading = value;
            }
        }

    }

    public class AccelerometerReading 
    {
        public AccelerometerReading (float accelerationX, float accelerationY, float accelerationZ)
        {
            this.AccelerationX = accelerationX;
            this.AccelerationY = accelerationY;
            this.AccelerationZ = accelerationZ;
        }
        public AccelerometerReading() { }
        public float AccelerationX { get; set; }
        public float AccelerationY { get; set; }
        public float AccelerationZ { get; set; }
    }
    public class ComboHprReading
    {
        public double Yaw { get; internal set; }
        public double Pitch { get; internal set; }
        public double Roll { get; internal set; }
        public double CompassHeading { get; internal set; }
        public DateTimeOffset TimeStamp { get; internal set; }
        public ComboHprFlags Flags { get; internal set; }
        public ComboHprReading(double yaw, double pitch, double roll, double compassHeading, ComboHprFlags flags, DateTimeOffset timestamp)
        {
            this.Yaw = yaw>360?yaw-360:yaw;
            this.Pitch = pitch > 360 ? pitch - 360 : pitch;
            this.Roll = roll > 360 ? roll- 360 : roll;

            this.Yaw = Yaw < 0 ? Yaw + 360 : Yaw;
            this.Pitch = Pitch < 0 ? Pitch + 360 : Pitch;
            this.Roll = Roll < 0 ? Roll + 360 : Roll;

            this.Yaw = Yaw < 0 ? Yaw + 360 : Yaw;
            this.Pitch = Pitch < 0 ? Pitch + 360 : Pitch;
            this.Roll = Roll < 0 ? Roll + 360 : Roll;


            if (Yaw < 0 || Pitch < 0 || Roll < 0)
                throw new ArgumentException();

            this.CompassHeading = compassHeading;
            this.TimeStamp = timestamp;

            this.Flags = flags;
        }
    }
}