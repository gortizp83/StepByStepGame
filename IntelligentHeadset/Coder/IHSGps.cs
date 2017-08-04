using IHSSdk.Internal;

namespace IHSSdk
{
    /// <summary>
    /// Container class for the GPU sensor properties of an Intelligent Headset device.
    /// </summary>
    public class IHSGps : NotifyPropertyChangedImpl
    {
        private double _latitude;
        public double Latitude
        {
            get
            {
                return _latitude;
            }
            set
            {
                if (_latitude != value)
                {
                    _latitude = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _longitude;
        public double Longitude
        {
            get
            {
                return _longitude;
            }
            set
            {
                if (_longitude != value)
                {
                    _longitude = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _altitude;
        public double Altitude
        {
            get
            {
                return _altitude;
            }
            set
            {
                if (_altitude != value)
                {
                    _altitude = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _horizontalAccuracy;
        public double HorizontalAccuracy
        {
            get
            {
                return _horizontalAccuracy;
            }
            set
            {
                if (_horizontalAccuracy != value)
                {
                    _horizontalAccuracy = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _signalIndicator;
        public double SignalIndicator
        {
            get
            {
                return _signalIndicator;
            }
            set
            {
                if (_signalIndicator != value)
                {
                    _signalIndicator = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
