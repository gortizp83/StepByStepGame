using IHSSdk.Internal;
using System;

namespace IHSSdk
{
    /// <summary>
    /// Container class for the sensor reading properties of an Intelligent Headset device.
    /// </summary>
    public class IHSSensorReadings : NotifyPropertyChangedImpl
    {
        private double _fusedHeading;
        public double FusedHeading
        {
            get
            {
                return _fusedHeading;
            }
            set
            {
                if (_fusedHeading != value)
                {
                    _fusedHeading = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _compassHeading;
        public double CompassHeading
        {
            get
            {
                return _compassHeading;
            }
            set
            {
                if (_compassHeading != value)
                {
                    _compassHeading = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _yaw;
        public double Yaw
        {
            get
            {
                return _yaw;
            }
            set
            {
                if (_yaw != value)
                {
                    _yaw = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _pitch;
        public double Pitch
        {
            get
            {
                return _pitch;
            }
            set
            {
                if (_pitch != value)
                {
                    _pitch = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _roll;
        public double Roll
        {
            get
            {
                return _roll;
            }
            set
            {
                if (_roll != value)
                {
                    _roll = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _accelerationX;
        public double AccelerationX
        {
            get
            {
                return _accelerationX;
            }
            set
            {
                if (_accelerationX != value)
                {
                    _accelerationX = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _accelerationY;
        public double AccelerationY
        {
            get
            {
                return _accelerationY;
            }
            set
            {
                if (_accelerationY != value)
                {
                    _accelerationY = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _accelerationZ;
        public double AccelerationZ
        {
            get
            {
                return _accelerationZ;
            }
            set
            {
                if (_accelerationZ != value)
                {
                    _accelerationZ = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _strengthOfMagneticField;
        public double StrengthOfMagneticField
        {
            get
            {
                return _strengthOfMagneticField;
            }
            set
            {
                if (_strengthOfMagneticField != value)
                {
                    _strengthOfMagneticField = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool _gyroCalibrated;
        public bool GyroCalibrated
        {
            get
            {
                return _gyroCalibrated;
            }
            set
            {
                _gyroCalibrated = value;
                NotifyPropertyChanged();
            }
        }

        private bool _magneticDisturbance;
        public bool MagneticDisturbance
        {
            get
            {
                return _magneticDisturbance;
            }
            set
            {
                _magneticDisturbance = value;
                NotifyPropertyChanged();
            }
        }
    }
}
