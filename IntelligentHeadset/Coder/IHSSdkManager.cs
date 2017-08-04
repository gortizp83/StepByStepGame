using IHSSdk.Internal;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace IHSSdk
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

    /// <summary>
    /// The main Intelligent Headset SDK interface.
    /// Used by applications running on foreground.
    /// </summary>
    public class IHSSdkManager
    {
        private const uint NumberOfAuthRetries = 5;

        /// <summary>
        /// Event for notifying application when the authentication fails.
        /// </summary>
        public event EventHandler<AuthenticationResult> AuthenticationFailed;

        /// <summary>
        /// This event is fired, when the list of sensors listened to is changed.
        /// </summary>
        public event EventHandler<Sensors> SensorsListenedToChanged
        {
            add
            {
                _engine.SensorsListenedToChanged += value;
            }
            remove
            {
                _engine.SensorsListenedToChanged -= value;
            }
        }

        private IHSEngine _engine;

        private static IHSSdkManager _instance;
        /// <summary>
        /// The singleton instance of this class.
        /// </summary>
        public static IHSSdkManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new IHSSdkManager();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Device information data container.
        /// </summary>
        public IHSDeviceInfo DeviceInfo
        {
            get
            {
                return _engine.DeviceInfo;
            }
        }

        /// <summary>
        /// Inertial measurement unit (IMU) data container.
        /// </summary>
        public IHSSensorReadings SensorReadings
        {
            get
            {
                return _engine.SensorReadings;
            }
        }

        /// <summary>
        /// GPS sensor data container.
        /// </summary>
        public IHSGps Gps
        {
            get
            {
                return _engine.Gps;
            }
        }

        /// <summary>
        /// List of discovered headsets.
        /// </summary>
        public ObservableCollection<IHSDevice> Headsets
        {
            get
            {
                return _engine.Devices;
            }
        }

        /// <summary>
        /// If true, will start listening to sensors when connected.
        /// </summary>
        public bool StartListeningToSensorsWhenConnected
        {
            get
            {
                return _engine.StartListeningToSensorsWhenConnected;
            }
            set
            {
                _engine.StartListeningToSensorsWhenConnected = value;
            }
        }

        /// <summary>
        /// Contains the sensors currently being listened to.
        /// </summary>
        public Sensors SensorsListenedTo
        {
            get
            {
                return _engine.SensorsListenedTo;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        private IHSSdkManager()
        {
            _engine = new IHSEngine();
            StartListeningToSensorsWhenConnected = true;
        }

        /// <summary>
        /// Starts the discovery of headsets.
        /// </summary>
        public async void DiscoverHeadsetsAsync()
        {
            await _engine.FindDevicesAsync();
        }

        /// <summary>
        /// Tries to connect to the given headset.
        /// </summary>
        /// <param name="headset">The IHSDevice instance defining the headset to connect to.</param>
        public async void ConnectAsync(IHSDevice headset)
        {
            DeviceInfo.Name = headset.Name;
            DeviceInfo.ConnectionStatus = ConnectionStatus.Connecting;

            await _engine.InitializeServicesAsync(headset);
            await _engine.ReadDeviceInformationAsync();

            uint retryCount = 0;

            AuthenticationResult authenticationResult = new AuthenticationResult()
            {
                Success = false
            };

            while (retryCount < NumberOfAuthRetries && !authenticationResult.Success)
            {
                try
                {
                    authenticationResult = await _engine.AuthenticateAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("ConnectAsync(): Failed to authenticate: " + ex.Message);
                }

                retryCount++;

                if (authenticationResult.Success)
                {
                    System.Diagnostics.Debug.WriteLine("ConnectAsync(): Authentication key successfully written");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ConnectAsync(): Failed to write the authentication key, "
                        + (NumberOfAuthRetries - retryCount) + " tries left");
                }
            }

            if (!authenticationResult.Success)
            {
                DeviceInfo.ConnectionStatus = ConnectionStatus.NotConnected;

                if (AuthenticationFailed != null)
                {
                    AuthenticationFailed(this, authenticationResult);
                }
            }
        }

        /// <summary>
        /// Starts listening to the given headset sensors.
        /// </summary>
        /// <param name="sensorsToListenTo">The sensors to start listening to.</param>
        public async void StartListeningToSensorsAsync(Sensors sensorsToListenTo = Sensors.Accelerometer | Sensors.Gyro | Sensors.Magnetometer | Sensors.Gps)
        {
            if (DeviceInfo.ConnectionStatus == ConnectionStatus.Connected)
            {
                await _engine.StartListeningToSensorReadingsAsync(sensorsToListenTo);
            }
        }

        /// <summary>
        /// Stops listening to the given headset sensors.
        /// </summary>
        /// <param name="sensorsToStopListeningTo">The sensors to stop listening to.</param>
        public async void StopListeningToSensorsAsync(Sensors sensorsToStopListeningTo = Sensors.Accelerometer | Sensors.Gyro | Sensors.Magnetometer | Sensors.Gps)
        {
            if (DeviceInfo.ConnectionStatus == ConnectionStatus.Connected)
            {
                await _engine.StopListeningToSensorReadingsAsync(sensorsToStopListeningTo);
            }
        }

        /// <summary>
        /// Reads current values from the given headset sensors.
        /// </summary>
        /// <param name="sensorsToRead">The sensors to read values of</param>
        public async Task<SensorReadingResult> ReadSensorValuesAsync(Sensors sensorsToRead = Sensors.Accelerometer | Sensors.Gyro | Sensors.Magnetometer | Sensors.Gps)
        {
            return await _engine.ReadSensorValuesAsync(sensorsToRead);
        }
    }
}
