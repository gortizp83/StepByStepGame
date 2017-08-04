using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Microsoft.PartnerCatalyst.IntelligentHeadset
{
    /// <summary>
    /// Manages retrieval and parsing of the sensor data provided by the headset.
    /// </summary>
    class IHSSensorReadingsHandler
    {
        /// <summary>
        /// This event is fired, when the list of sensors listened to is changed.
        /// </summary>
        public event EventHandler<Sensors> SensorsListenedToChanged;

        private IHSSensorReadings _sensorReadings;
        private IHSGps _gpsReading;
        private GattDeviceService _gnImuService;
        private GattDeviceService _gnGPSservice;
        private GattDeviceService _gnSystemService;
        private GattCharacteristic _accelerometerCharacteristic = null;
        private GattCharacteristic _comboHprCharacteristic = null; // Gyro
        private GattCharacteristic _magnetometerCalibrationCharacteristic = null;
        private GattCharacteristic _magnetometerCharacteristic = null;
        private GattCharacteristic _gpsAllInOneCharacteristic = null;
        private bool _startingToListen = false;

        /// <summary>
        /// Has the sensors currently being listened to.
        /// </summary>
        public Sensors SensorsListenedTo
        {
            get;
            private set;
        }

        public IHSSensorReadingsHandler(IHSSensorReadings sensorReadings, IHSGps Gps, GattDeviceService gnImuService, GattDeviceService gnGPSservice, GattDeviceService gnSystemService)
        {
            _sensorReadings = sensorReadings;
            _gpsReading = Gps;
            _gnImuService = gnImuService;
            _gnGPSservice = gnGPSservice;
            _gnSystemService = gnSystemService;
            GetGattCharacteristicsForImuService();
            GetGattCharacteristicsForGPSService();
            SensorsListenedTo = Sensors.None;
        }

        /// <summary>
        /// Starts listening to the specified sensors.
        /// </summary>
        /// <param name="sensorsToListenTo">The sensors to start listening to.</param>
        /// <returns></returns>
        public async Task StartListeningToSensorReadingsAsync(Sensors sensorsToListenTo)
        {
            if (!_startingToListen)
            {
                _startingToListen = true;
                System.Diagnostics.Debug.WriteLine("StartListeningToSensorReadingsAsync(): " + sensorsToListenTo);

                if (sensorsToListenTo.HasFlag(Sensors.Accelerometer)
                    && !SensorsListenedTo.HasFlag(Sensors.Accelerometer))
                {
                    await _accelerometerCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.Notify);

                    _accelerometerCharacteristic.ValueChanged += OnAccelerationVectorValueChangedAsync;
                }

                if (sensorsToListenTo.HasFlag(Sensors.Gyro)
                    && !SensorsListenedTo.HasFlag(Sensors.Gyro))
                {
                    await _comboHprCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.Notify);

                    _comboHprCharacteristic.ValueChanged += OnComboHprCharacteristicValueChangedAsync;
                }

                if (sensorsToListenTo.HasFlag(Sensors.Magnetometer)
                    && !SensorsListenedTo.HasFlag(Sensors.Magnetometer))
                {
                    await _magnetometerCalibrationCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.Notify);

                    _magnetometerCalibrationCharacteristic.ValueChanged += OnCalibrationCharacteristicValueChangedAsync;

                    await _magnetometerCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.Notify);

                    _magnetometerCharacteristic.ValueChanged += OnMagnetometerCharacteristicValueChangedAsync;
                }

                if (sensorsToListenTo.HasFlag(Sensors.Gps)
                     && !SensorsListenedTo.HasFlag(Sensors.Gps))
                {
                    await _gpsAllInOneCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.Notify);

                    _gpsAllInOneCharacteristic.ValueChanged += OnGPSCharacteristicValueChangedAsync;
                }

                SensorsListenedTo |= sensorsToListenTo;

                if (SensorsListenedToChanged != null)
                {
                    SensorsListenedToChanged(this, SensorsListenedTo);
                }

                _startingToListen = false;
            }
        }

        /// <summary>
        /// Stops listening to the specified sensors.
        /// </summary>
        /// <param name="sensorsToStopListeningTo">The sensors to stop listening to.</param>
        /// <returns></returns>
        public async Task StopListeningToSensorReadingsAsync(Sensors sensorsToStopListeningTo)
        {
            
            if (sensorsToStopListeningTo.HasFlag(Sensors.Accelerometer)
                && SensorsListenedTo.HasFlag(Sensors.Accelerometer))
            {
                await _accelerometerCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.None);

                //_accelerometerCharacteristic.ValueChanged -= OnAccelerationVectorValueChangedAsync;
            }

            if (sensorsToStopListeningTo.HasFlag(Sensors.Gyro)
                && SensorsListenedTo.HasFlag(Sensors.Gyro))
            {
                await _comboHprCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.None);

                //_comboHprCharacteristic.ValueChanged -= OnComboHprCharacteristicValueChanged;
            }

            if (sensorsToStopListeningTo.HasFlag(Sensors.Magnetometer)
                && SensorsListenedTo.HasFlag(Sensors.Magnetometer))
            {
                await _magnetometerCalibrationCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.None);

                await _magnetometerCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.None);

                //_magnetometerCharacteristic.ValueChanged -= OnMagnetometerCharacteristicValueChanged;
            }

            if (sensorsToStopListeningTo.HasFlag(Sensors.Gps)
                && SensorsListenedTo.HasFlag(Sensors.Gps))
            {
                await _gpsAllInOneCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.None);

                //_gpsAllInOneCharacteristic.ValueChanged -= OnGPSCharacteristicValueChanged;
            }

            SensorsListenedTo &= ~sensorsToStopListeningTo;

            if (SensorsListenedToChanged != null)
            {
                SensorsListenedToChanged(this, SensorsListenedTo);
            }
        }
        /// <summary>
        /// Reads current values from the given headset sensors.
        /// </summary>
        /// <param name="sensorsToRead">The sensors to read values of</param>
        /// <returns></returns>
        public async Task<SensorReadingResult> ReadSensorValuesAsync(Sensors sensorsToRead)
        {
            SensorReadingResult sensorReadingResult = new SensorReadingResult()
            {
                Success = true
            };

            if (sensorsToRead.HasFlag(Sensors.Accelerometer)
                && !SensorsListenedTo.HasFlag(Sensors.Accelerometer)) //Don't read values that we are listening already
            {
                try
                {
                    GattReadResult readResult = await _accelerometerCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                    byte[] data = WindowsRuntimeBufferExtensions.ToArray(readResult.Value);
                    ResolveAccelerometerData(data);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("ReadSensorValuesAsync(): " + e.Message);
                    sensorReadingResult.Exception = e;
                    sensorReadingResult.Success = false;
                    sensorReadingResult.SensorsFailed |= Sensors.Accelerometer;
                }
            }

            if (sensorsToRead.HasFlag(Sensors.Gyro)
                && !SensorsListenedTo.HasFlag(Sensors.Gyro))
            {
                try
                {
                    GattReadResult readResult = await _comboHprCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                    byte[] data = WindowsRuntimeBufferExtensions.ToArray(readResult.Value);
                    ResolveComboHprData(data);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("ReadSensorValuesAsync(): " + e.Message);
                    sensorReadingResult.Exception = e;
                    sensorReadingResult.Success = false;
                    sensorReadingResult.SensorsFailed |= Sensors.Gyro;
                }
            }

            if (sensorsToRead.HasFlag(Sensors.Magnetometer)
                && !SensorsListenedTo.HasFlag(Sensors.Magnetometer))
            {
                try
                {
                    GattReadResult readResult = await _magnetometerCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                    byte[] data = WindowsRuntimeBufferExtensions.ToArray(readResult.Value);
                    ResolveMagnetometerData(data);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("ReadSensorValuesAsync(): " + e.Message);
                    sensorReadingResult.Exception = e;
                    sensorReadingResult.Success = false;
                    sensorReadingResult.SensorsFailed |= Sensors.Magnetometer;
                }

            }

            if (sensorsToRead.HasFlag(Sensors.Gps)
                 && !SensorsListenedTo.HasFlag(Sensors.Gps))
            {
                try
                {
                    GattReadResult readResult = await _gpsAllInOneCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                    byte[] data = WindowsRuntimeBufferExtensions.ToArray(readResult.Value);
                    ResolveGPSData(data);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("ReadSensorValuesAsync(): " + e.Message);
                    sensorReadingResult.Exception = e;
                    sensorReadingResult.Success = false;
                    sensorReadingResult.SensorsFailed |= Sensors.Gps;
                }
              }

            return sensorReadingResult;

        }

        private void GetGattCharacteristicsForImuService()
        {
            IReadOnlyList<GattCharacteristic> characteristicList =
                _gnImuService.GetAllCharacteristics();

            foreach (GattCharacteristic characteristic in characteristicList)
            {
                if (characteristic.Uuid == Guid.Parse(IHSUuids.AccVector))
                {
                    _accelerometerCharacteristic = characteristic;

                    System.Diagnostics.Debug.WriteLine(
                        "GetGattCharacteristicsForSensors(): Accelerometer GATT characteristic properties: "
                        + _accelerometerCharacteristic.CharacteristicProperties);
                }
                else if (characteristic.Uuid == Guid.Parse(IHSUuids.ComboHpr))
                {
                    _comboHprCharacteristic = characteristic;

                    System.Diagnostics.Debug.WriteLine(
                        "GetGattCharacteristicsForSensors(): Gyro GATT characteristic properties: "
                        + _comboHprCharacteristic.CharacteristicProperties);
                }
                else if (characteristic.Uuid == Guid.Parse(IHSUuids.Calibration))
                {
                    _magnetometerCalibrationCharacteristic = characteristic;

                    System.Diagnostics.Debug.WriteLine(
                        "GetGattCharacteristicsForSensors(): Magnetometer calibration GATT characteristic properties: "
                        + _magnetometerCalibrationCharacteristic.CharacteristicProperties);
                }
                else if (characteristic.Uuid == Guid.Parse(IHSUuids.MagVector))
                {
                    _magnetometerCharacteristic = characteristic;

                    System.Diagnostics.Debug.WriteLine(
                        "GetGattCharacteristicsForSensors(): Magnetometer GATT characteristic properties: "
                        + _magnetometerCharacteristic.CharacteristicProperties);
                }
            }
        }

        private void GetGattCharacteristicsForGPSService()
        {
            IReadOnlyList<GattCharacteristic> characteristicList =
                _gnGPSservice.GetAllCharacteristics();

            foreach (GattCharacteristic characteristic in characteristicList)
            {
                if (characteristic.Uuid == Guid.Parse(IHSUuids.AllInOne))
                {
                    _gpsAllInOneCharacteristic = characteristic;

                    System.Diagnostics.Debug.WriteLine(
                        "GetGattCharacteristicsForGPSService(): GPS GATT characteristic properties: "
                        + _accelerometerCharacteristic.CharacteristicProperties);
                }
            }
        }

        private async void OnAccelerationVectorValueChangedAsync(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] data = args.CharacteristicValue.ToArray();

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ResolveAccelerometerData(data);
                });
        }

        private async void OnComboHprCharacteristicValueChangedAsync(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] data = args.CharacteristicValue.ToArray();

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ResolveComboHprData(data);
                });
        }

        private async void OnCalibrationCharacteristicValueChangedAsync(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] data = args.CharacteristicValue.ToArray();

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ResolveMagnetometerCalibrationData(data);
                });
        }

        private async void OnMagnetometerCharacteristicValueChangedAsync(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] data = args.CharacteristicValue.ToArray();

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ResolveMagnetometerData(data);
                });
        }

        private async void OnGPSCharacteristicValueChangedAsync(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] data = args.CharacteristicValue.ToArray();

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ResolveGPSData(data);
                });
        }

        /// <summary>
        /// Extracts the accelerometer data from the given byte array.
        /// </summary>
        /// <param name="data">The accelerometer data as byte array.</param>
        private void ResolveAccelerometerData(byte[] data)
        {
            if (data != null && data.Length >= 6)
            {
                Int16 accelerationX = BitConverter.ToInt16(data, 0);
                Int16 accelerationY = BitConverter.ToInt16(data, 2);
                Int16 accelerationZ = BitConverter.ToInt16(data, 4);

                double maxValueOfInt16 = Int16.MaxValue;

                _sensorReadings.AccelerationX = Math.Min(Math.Max((accelerationX + maxValueOfInt16 / 2) / maxValueOfInt16, 0.0d), 1.0d);
                _sensorReadings.AccelerationY = Math.Min(Math.Max((accelerationY + maxValueOfInt16 / 2) / maxValueOfInt16, 0.0d), 1.0d);
                _sensorReadings.AccelerationZ = Math.Min(Math.Max((accelerationZ + maxValueOfInt16 / 2) / maxValueOfInt16, 0.0d), 1.0d);

                /*System.Diagnostics.Debug.WriteLine("ResolveAccelerometerData(): ["
                    + accelerationX + ", " + accelerationY + ", " + accelerationZ + "] -> ["
                    + _IHSSensorReadings.AccelerationX + "; "
                    + _IHSSensorReadings.AccelerationY + "; "
                    + _IHSSensorReadings.AccelerationZ + "]");*/
            }
        }

        /// <summary>
        /// Extracts the combo HPR data from the given byte array.
        /// </summary>
        /// <param name="data">The combo HPR data as byte array.</param>
        private void ResolveComboHprData(byte[] data)
        {
            if (data != null && data.Length >= 14)
            {
                ushort fusedHeading = BitConverter.ToUInt16(data, 0);
                Int16 pitch = BitConverter.ToInt16(data, 2);
                Int16 roll = BitConverter.ToInt16(data, 4);
                byte pace = data[6];
                byte flags = data[7];
                ushort fieldStrength = BitConverter.ToUInt16(data, 8);
                ushort yaw = BitConverter.ToUInt16(data, 10);
                ushort cheading = BitConverter.ToUInt16(data, 12);

                _sensorReadings.FusedHeading = (double)fusedHeading / 10;
                _sensorReadings.MagneticDisturbance = (flags == 0x02);
                _sensorReadings.Pitch = (double)pitch / 10;
                _sensorReadings.Roll = (double)roll / 10;
                _sensorReadings.Yaw = (double)yaw / 10;
                _sensorReadings.CompassHeading = (double)cheading / 10;
            }
        }

        /// <summary>
        /// Extracts the magnetometer data from the given byte array.
        /// </summary>
        /// <param name="data">The magnetometer data as byte array.</param>
        private void ResolveMagnetometerData(byte[]data)
        {
            if (data != null && data.Length >= 8)
            {
                Int16 magnetometerX = BitConverter.ToInt16(data, 0);
                Int16 magnetometerY = BitConverter.ToInt16(data, 2);
                Int16 magnetometerZ = BitConverter.ToInt16(data, 4);
                UInt16 fieldStrength = BitConverter.ToUInt16(data, 6);

                _sensorReadings.StrengthOfMagneticField = fieldStrength;

                // TODO: Calculate heading
            }
        }

        /// <summary>
        /// Extracts the magnetometer parameters data from the given byte array.
        /// </summary>
        /// <param name="data">The magnetometer parameters data as byte array.</param>
        private void ResolveMagParamsData(byte[] data)
        {
            if (data != null && data.Length >= 6)
            {
                UInt16 fieldIntensity = BitConverter.ToUInt16(data, 0);
                Int16 inclination = BitConverter.ToInt16(data, 2);
                Int16 declination = BitConverter.ToInt16(data, 4);

                // TODO: Store the data somewhere
            }
        }

        /// <summary>
        /// Extracts the magnetometer calibration data from the given byte array.
        /// </summary>
        /// <param name="data">The magnetometer calibration data as byte array.</param>
        private void ResolveMagnetometerCalibrationData(byte[] data)
        {
            if (data != null && data.Length >= 7)
            {
                // TODO
                //uint8 status; int16 x; int16 y; int16 z;
            }
        }

        /// <summary>
        /// Extracts GPS data from the given byte array.
        /// </summary>
        /// <param name="data">The magnetometer data as byte array.</param>
        private void ResolveGPSData(byte[] data)
        {
            if (data != null && data.Length >= 16)
            {
                Int32 latitude = BitConverter.ToInt32(data, 0);
                Int32 longitude = BitConverter.ToInt32(data, 4);
                UInt32 timeAltFix = BitConverter.ToUInt32(data, 8);
                UInt32 speedCourseHorizerror = BitConverter.ToUInt32(data, 12);
                
                //timeAltFix content:
                //bit(0‐16): seconds sincemidnight, bit17‐28: altitude(0..4095m), bit29‐31: fixtype(0..7)
                UInt32 secondsSincemidnight = timeAltFix & 0x1FFFF;
                UInt32 altitude = (timeAltFix >> 17 ) & 0x7FFF;
                UInt32 fixtype = (timeAltFix >> 29) & 0x7;

                //speedCourseHorizerror content:
                //bit 0‐10: speed (x0.1km/t) ‐ 0‐204.7,  bit 11‐19: course (0‐359), bit 20‐25: Horizontal error (0‐31.5m, LSB=0.5m), 
                //bit 26‐30: satellite count (0..31),  bit 31: data valid
                UInt32 speed = speedCourseHorizerror & 0x3FF;
                UInt32 course = (speedCourseHorizerror >> 11) & 0x1FF;
                UInt32 horizontalErrorLSB = (speedCourseHorizerror >> 20) & 0x1;
                UInt32 horizontalError = (speedCourseHorizerror >> 21) & 0x1F;
                UInt32 satelliteCount = (speedCourseHorizerror >> 26) & 0x1F;
                UInt32 dataValid = (speedCourseHorizerror >> 31) & 0x1;

                _gpsReading.Latitude = (double)latitude / 600000;
                _gpsReading.Longitude = (double)longitude / 600000;
                _gpsReading.Altitude = altitude;
                _gpsReading.HorizontalAccuracy = (double)horizontalError + (horizontalErrorLSB == 1 ? 0.5 : 0);
            }
        }
    }
}
