using IHSSdk.Internal;

namespace IHSSdk
{
    public enum ConnectionStatus
    {
        NotConnected = 0,
        Connecting = 1,
        Connected = 2
    };

    /// <summary>
    /// Container class for device information properties of an Intelligent Headset device.
    /// </summary>
    public class IHSDeviceInfo : NotifyPropertyChangedImpl
    {
        private string _name = "n/a";
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }

        private ConnectionStatus _connectionStatus;
        public ConnectionStatus ConnectionStatus
        {
            get
            {
                return _connectionStatus;
            }
            set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private uint _rssi;
        public uint Rssi
        {
            get
            {
                return _rssi;
            }
            set
            {
                if (_rssi != value)
                {
                    _rssi = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _hardwareRevision = "n/a";
        public string HardwareRevision
        {
            get
            {
                return _hardwareRevision;
            }
            set
            {
                _hardwareRevision = value;
                NotifyPropertyChanged();
            }
        }

        private string _firmwareRevision = "n/a";
        public string FirmwareRevision
        {
            get
            {
                return _firmwareRevision;
            }
            set
            {
                _firmwareRevision = value;
                NotifyPropertyChanged();
            }
        }

        private string _softwareRevision = "n/a";
        public string SoftwareRevision
        {
            get
            {
                return _softwareRevision;
            }
            set
            {
                _softwareRevision = value;
                NotifyPropertyChanged();
            }
        }

        private string _apiRevision = "n/a";
        public string ApiRevision
        {
            get
            {
                return _apiRevision;
            }
            set
            {
                _apiRevision = value;
                NotifyPropertyChanged();
            }
        }

        private string _modelNumber = "n/a";
        public string ModelNumber
        {
            get
            {
                return _modelNumber;
            }
            set
            {
                _modelNumber = value;
                NotifyPropertyChanged();
            }
        }

        private string _serialnumber = "n/a";
        public string SerialNumber
        {
            get
            {
                return _serialnumber;
            }
            set
            {
                _serialnumber = value;
                NotifyPropertyChanged();
            }
        }
    }
}
