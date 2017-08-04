using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace Microsoft.PartnerCatalyst.IntelligentHeadset
{
    public class IHSEngine
    {
        /// <summary>
        /// This event is fired, when the list of sensors listened to is changed.
        /// </summary>
        public event EventHandler<Sensors> SensorsListenedToChanged;

        private GattDeviceService _gnImuService;
        private GattDeviceService _gnSystemService;
        private GattDeviceService _gnGPSservice;
        private GattDeviceService _deviceInfoService;
        private GattDeviceService _batteryInfoService;
        private DeviceInformationCollection _deviceListGnImuService;
        private IHSDeviceInfoHandler _deviceInfoHandler;
        private IHSSensorReadingsHandler _sensorReadingsHandler;
        private uint _nonce;

        /// <summary>
        /// Device information data container.
        /// </summary>
        public IHSDeviceInfo DeviceInfo
        {
            get;
            private set;
        }

        /// <summary>
        /// Inertial measurement unit (IMU) data container.
        /// </summary>
        public IHSSensorReadings SensorReadings
        {
            get;
            private set;
        }

        /// <summary>
        /// GPS sensor data container.
        /// </summary>
        public IHSGps Gps
        {
            get;
            private set;
        }

        /// <summary>
        /// List of discovered headsets.
        /// </summary>
        public ObservableCollection<IHSDevice> Devices
        {
            get;
            private set;
        }

        /// <summary>
        /// Indicates whether the GATT device services have been acquired or not.
        /// </summary>
        public bool IsInitialized
        {
            get;
            private set;
        }

        /// <summary>
        /// If true, will start listening to sensors when connected.
        /// </summary>
        public bool StartListeningToSensorsWhenConnected
        {
            get;
            set;
        }

        /// <summary>
        /// Contains the sensors currently being listened to.
        /// </summary>
        public Sensors SensorsListenedTo
        {
            get
            {
                if (_sensorReadingsHandler != null)
                {
                    return _sensorReadingsHandler.SensorsListenedTo;
                }

                return Sensors.None;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public IHSEngine()
        {
            Devices = new ObservableCollection<IHSDevice>();

            DeviceInfo = new IHSDeviceInfo();
            SensorReadings = new IHSSensorReadings();
            Gps = new IHSGps();
        }

        /// <summary>
        /// Discovers the bluetooth headsets and populates the devices list.
        /// </summary>
        public async Task FindDevicesAsync()
        {
            Devices.Clear();

            _deviceListGnImuService = await DeviceInformation.FindAllAsync(
                GattDeviceService.GetDeviceSelectorFromUuid(Guid.Parse(IHSUuids.GnImuService)));

            if (_deviceListGnImuService.Count > 0)
            {
                foreach (var deviceInformation in _deviceListGnImuService)
                {
                    IHSDevice device = new IHSDevice(deviceInformation);
                    Devices.Add(device);
                }
            }
        }

        /// <summary>
        /// Initializes (acquires) the GATT services.
        /// </summary>
        /// <param name="device">The headset device whose services to initialize.</param>
        /// <returns>True, if successful. False otherwise.</returns>
        public async Task<bool> InitializeServicesAsync(IHSDevice device)
        {
            if (_deviceInfoService != null)
            {
                _deviceInfoService.Device.ConnectionStatusChanged -= OnDeviceConnectionStatusChangedAsync;
            }

            try
            {
                _gnImuService = await GattDeviceService.FromIdAsync(device.DeviceInformation.Id);
                _gnSystemService = _gnImuService.Device.GetGattService(Guid.Parse(IHSUuids.GnSystemService));
                _gnGPSservice = _gnImuService.Device.GetGattService(Guid.Parse(IHSUuids.GnGpsService));
                _deviceInfoService = _gnImuService.Device.GetGattService(IHSUuids.CreateGuidfromWellKnownUUID(IHSUuids.DeviceInfoServiceId));
                _batteryInfoService = _gnImuService.Device.GetGattService(IHSUuids.CreateGuidfromWellKnownUUID(IHSUuids.BatteryServiceId));
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("InitializeServiceAsync(): " + e.Message);
            }

            if (_deviceInfoService != null)
            {
                _deviceInfoService.Device.ConnectionStatusChanged += OnDeviceConnectionStatusChangedAsync;
                _deviceInfoHandler = new IHSDeviceInfoHandler(DeviceInfo, _deviceInfoService, _batteryInfoService);
            }

            if (_gnImuService != null && _gnGPSservice != null && _gnSystemService != null)
            {
                _sensorReadingsHandler = new IHSSensorReadingsHandler(SensorReadings, Gps, _gnImuService, _gnGPSservice, _gnSystemService);
                _sensorReadingsHandler.SensorsListenedToChanged += SensorsListenedToChanged;
            }

            System.Diagnostics.Debug.WriteLine("InitializeServiceAsync(): Status:"
                + "\n\t- GN system service: " + ((_gnSystemService == null) ? "FAIL" : "OK")
                + "\n\t- Device information service: " + ((_deviceInfoService == null) ? "FAIL" : "OK")
                + "\n\t- Battery informatiojn service: " + ((_batteryInfoService == null) ? "FAIL" : "OK")
                + "\n\t- GN IMU service: " + ((_gnImuService == null) ? "FAIL" : "OK")
                + "\n\t- GN GPS service: " + ((_gnGPSservice == null) ? "FAIL" : "OK"));

            IsInitialized = (_deviceInfoService != null && _gnImuService != null);
            return IsInitialized;
        }

        /// <summary>
        /// Reads the nonce value from the headset, calculates a response and tries to send it
        /// back. Note that this method is prone to fail, so you might have to do several
        /// tries.
        /// </summary>
        /// <returns>The authentication result.</returns>
        public async Task<AuthenticationResult> AuthenticateAsync()
        {
            AuthenticationResult result = new AuthenticationResult()
            {
                Success = false,
                Exception = null
            };

            if (await ReadDeviceGnSystemServiceAsync())
            {
                uint authKey = (uint)IHSAuthHelper.CalculateAuthenticationKey(_nonce);

                IReadOnlyList<GattCharacteristic> characteristicList =
                    _gnSystemService.GetCharacteristics(Guid.Parse(IHSUuids.Key));

                foreach (GattCharacteristic characteristic in characteristicList)
                {
                    if (characteristic.Uuid == Guid.Parse(IHSUuids.Key))
                    {
                        byte[] authKeyAsByteArray = BitConverter.GetBytes(authKey);

                        try
                        {
                            await characteristic.WriteValueAsync(authKeyAsByteArray.AsBuffer(), GattWriteOption.WriteWithResponse);
                            result.Success = true;
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine("AuthenticateAsync(): " + e.Message);
                            result.Exception = e;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Resolves the device and battery information.
        /// </summary>
        public async Task ReadDeviceInformationAsync()
        {
            if (IsInitialized)
            {
                await _deviceInfoHandler.ReadDeviceInformationAsync();
                await _deviceInfoHandler.ReadDeviceBatteryInformationAsync();
            }
        }

        /// <summary>
        /// Starts listening to the given sensor readings.
        /// </summary>
        /// <param name="sensorsToListenTo">The sensors to start listening to.</param>
        public async Task StartListeningToSensorReadingsAsync(Sensors sensorsToListenTo)
        {
            if (IsInitialized)
            {
                await _sensorReadingsHandler.StartListeningToSensorReadingsAsync(sensorsToListenTo);
            }
        }

        /// <summary>
        /// Stops listening to the given sensor readings.
        /// </summary>
        /// <param name="sensorsToStopListeningTo">The sensors to stop listening to.</param>
        public async Task StopListeningToSensorReadingsAsync(Sensors sensorsToStopListeningTo)
        {
            if (IsInitialized)
            {
                await _sensorReadingsHandler.StopListeningToSensorReadingsAsync(sensorsToStopListeningTo);
            }
        }

        /// <summary>
        ///  Reads current values from the given headset sensors.
        /// </summary>
        /// <param name="sensorsToRead">The sensors to read values of</param>
        public async Task<SensorReadingResult> ReadSensorValuesAsync(Sensors sensorsToRead)
        {
            if (IsInitialized)
            {
                return await _sensorReadingsHandler.ReadSensorValuesAsync(sensorsToRead);
            }
            else
            {
                return new SensorReadingResult()
                {
                    Success = false
                };
            }
        }

        /// <summary>
        /// Resolves the nonce value from the connected headset.
        /// </summary>
        /// <returns>True, if successful. False otherwise.</returns>
        private async Task<bool> ReadDeviceGnSystemServiceAsync()
        {
            bool success = false;

            if (IsInitialized)
            {
                IReadOnlyList<GattCharacteristic> characteristicList = _gnSystemService.GetAllCharacteristics();

                foreach (var characteristic in characteristicList)
                {
                    if (characteristic.Uuid == Guid.Parse(IHSUuids.Nonce))
                    {
                        GattReadResult readResult = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                        byte[] data = WindowsRuntimeBufferExtensions.ToArray(readResult.Value);
                        _nonce = BitConverter.ToUInt32(data, 0);
                        success = true;
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// Handles connection status changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OnDeviceConnectionStatusChangedAsync(BluetoothLEDevice sender, object args)
        {
            System.Diagnostics.Debug.WriteLine("OnDeviceConnectionStatusChanged(): " + _deviceInfoService.Device.ConnectionStatus);
            ConnectionStatus connectionStatus = ConnectionStatus.NotConnected;

            switch (_deviceInfoService.Device.ConnectionStatus)
            {
                case BluetoothConnectionStatus.Connected:
                    connectionStatus = ConnectionStatus.Connected;
                    break;
                case BluetoothConnectionStatus.Disconnected:
                    connectionStatus = ConnectionStatus.NotConnected;
                    break;
            }
            
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    DeviceInfo.ConnectionStatus = connectionStatus;
                });
            
            if (StartListeningToSensorsWhenConnected && connectionStatus == ConnectionStatus.Connected)
            {
                await StartListeningToSensorReadingsAsync(Sensors.Accelerometer | Sensors.Gyro | Sensors.Magnetometer | Sensors.Gps);
            }
        }
    }
}
