//
// IntelligentHeadsetSensorPeripheral.cs
//
// Copyright © 2015 Microsoft Corporation. All rights reserved.
// Created by blambert, October 2015. 
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Microsoft.PartnerCatalyst.NavigationSensorPeripheral;
using System.Numerics;
using Windows.Storage.Streams;
using System.Diagnostics;

namespace Microsoft.PartnerCatalyst.IntelligentHeadset
{
    // IntelligentHeadsetSensorPeripheral class.
    public class IntelligentHeadsetSensorPeripheral : INavigationSensorPeripheral
    {
        /// <summary>
        /// The shared secret for authentication.
        /// </summary>
        private const uint SharedSecret = 0xE47325F4;

        /// <summary>
        /// GATT services we use.
        /// </summary>
        private GattDeviceService _imuService;
        private GattDeviceService _systemService;
        private GattDeviceService _gpsService;
        private GattDeviceService _deviceInfoService;
        private GattDeviceService _batteryInfoService;

      
        /// <summary>
        /// GATT service characteristics we use.
        /// </summary>
        private GattCharacteristic _magnetometerCalibrationCharacteristic;
        private GattCharacteristic _magnetometerCharacteristic;
        private GattCharacteristic _comboHprCharacteristic;                 // Gyro
        private GattCharacteristic _accelerometerCharacteristic;
        private GattCharacteristic _gpsAllInOneCharacteristic;




        /// <summary>
        /// ConnectionFailed event. 
        /// </summary>
        public event TypedEventHandler<INavigationSensorPeripheral, ConnectionFailedEventArgs> ConnectionFailed;

        /// <summary>
        /// ConnectionChanged event. 
        /// </summary>
        public event TypedEventHandler<INavigationSensorPeripheral, EventArgs> ConnectionChanged;
        
        /// <summary>
        /// The GeocoordinateChanged event. Raised whenever a GPS location arrives from the Intelligent Headset.
        /// </summary>
        public event TypedEventHandler<INavigationSensorPeripheral, GeocoordinateChangedEventArgs> GeocoordinateChanged;

        /// <summary>
        /// The CompassChanged event. Raised whenever a compass heading arrives from the Intelligent Headset.
        /// </summary>
        public event TypedEventHandler<INavigationSensorPeripheral, CompassChangedEventArgs> CompassChanged;

        public event TypedEventHandler<INavigationSensorPeripheral, GyroChangedEventArgs> GyroChanged;

        public event TypedEventHandler<INavigationSensorPeripheral, AccelerometerValueChangedEventArgs> AccelerometerValueChanged;

        /// <summary>
        /// Finds all Intelligent Headset devices that are paired on the device.
        /// </summary>
        /// <remarks>You may need to call this from time to time to find any newly connected or newly disconnected Intelligent Headset devices.</remarks>
        /// <returns>An IEnumerable of IntelligentHeadsetSensorDevice objects representing Intelligent Headset devices that are paired on the device.</returns>
        public static async Task<IEnumerable<IntelligentHeadsetSensorDevice>> FindDevicesAsync()
        {
            var result = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(DeviceClass.AudioRender);

            foreach(var r in result)
            {
                Debug.WriteLine($"id:{r.Id}, Name: {r.Name}");
            }

            // Create a device selector for devices advertising the IMU service.
            var deviceSelector = GattDeviceService.GetDeviceSelectorFromUuid(Guid.Parse(Identifiers.GnImuService)); //GnGpsService

            // Asynchonously obtain the device information collection for devices advertising the IMU service.
            DeviceInformationCollection deviceInformationCollection = await DeviceInformation.FindAllAsync(deviceSelector, new string[] { "System.Devices.Connected" });

            // Return the devices.
            return deviceInformationCollection.Select(deviceInformation => new IntelligentHeadsetSensorDevice(deviceInformation)).ToList<IntelligentHeadsetSensorDevice>();
        }
        
        /// <summary>
        /// Gets a IntelligentHeadsetSensorDevice representing the Intelligent Headset device.
        /// </summary>
        public IntelligentHeadsetSensorDevice IntelligentHeadsetSensorDevice { get; }

        /// <summary>
        /// Gets a value which indicates whether we're attached to the Intelligent Headset device.
        /// </summary>
        public bool Attached { get; private set; }

        /// <summary>
        /// Gets a value which indicates whether we're connected to the Intelligent Headset device.
        /// </summary>
        public bool Connected { get; private set; }

        /// <summary>
        /// Initializes a new IntelligentHeadsetSensorPeripheral.
        /// </summary>
        /// <param name="intelligentHeadsetDevice">The Intelligent Headset device.</param>
        public IntelligentHeadsetSensorPeripheral(IntelligentHeadsetSensorDevice intelligentHeadsetDevice)
        {
            IntelligentHeadsetSensorDevice = intelligentHeadsetDevice;
        }

        /// <summary>
        /// Attaches the Intelligent Headset device.
        /// </summary>
        /// <remarks>This metod should not be called from multiple threads.</remarks>
        /// <returns>The task.</returns>
        public async Task AttachAsync()
        {
            // Get the device ID and the device name.
            var deviceId = IntelligentHeadsetSensorDevice.DeviceInformation.Id;
            var deviceName = IntelligentHeadsetSensorDevice.DeviceInformation.Name;

            // If we're already attached, throw an exception.
            if (Attached)
            {
                // Trace.
                Trace("Already attached to " + deviceName);

                // Throw exception.
                throw new IntelligentHeadsetSensorPeripheralException(
                    IntelligentHeadsetSensorPeripheralException.ErrorCode.AlreadyAttached,
                    "Already attached to " + deviceName,
                    null);
            }

            // Try to attach to the Intelligent Headset.
            try
            {
                // Trace.
                Trace("Attempting to attached to " + IntelligentHeadsetSensorDevice.DeviceInformation.Name);

                // Get the device service. If it's not available, return false.
                _imuService = await GattDeviceService.FromIdAsync(deviceId);
                if (_imuService == null)
                {
                    // Trace.
                    Trace("Unable to get device service for " + deviceName);

                    // Throw IntelligentHeadsetSensorPeripheralException.
                    throw new IntelligentHeadsetSensorPeripheralException(
                        IntelligentHeadsetSensorPeripheralException.ErrorCode.AttachFailed,
                        "Unable to get device service for " + deviceName,
                        null);
                }

                // Get the services we use.
                _systemService = _imuService.Device.GetGattService(Guid.Parse(Identifiers.GnSystemService));
                _gpsService = _imuService.Device.GetGattService(Guid.Parse(Identifiers.GnGpsService));
                _deviceInfoService = _imuService.Device.GetGattService(Identifiers.CreateGuidfromWellKnownUUID(Identifiers.DeviceInfoServiceId));
                _batteryInfoService = _imuService.Device.GetGattService(Identifiers.CreateGuidfromWellKnownUUID(Identifiers.BatteryServiceId));

                // Get the characteristics we uses from the IMU service.
                _magnetometerCalibrationCharacteristic = _imuService.GetCharacteristics(Guid.Parse(Identifiers.Calibration))?.FirstOrDefault();
                _magnetometerCharacteristic = _imuService.GetCharacteristics(Guid.Parse(Identifiers.MagVector))?.FirstOrDefault();
                _comboHprCharacteristic = _imuService.GetCharacteristics(Guid.Parse(Identifiers.ComboHpr))?.FirstOrDefault();
                _accelerometerCharacteristic = _imuService.GetCharacteristics(Guid.Parse(Identifiers.AccVector))?.FirstOrDefault();

                // Get the characteristics we use from the GPS service.
                _gpsAllInOneCharacteristic = _gpsService.GetCharacteristics(Guid.Parse(Identifiers.AllInOne))?.FirstOrDefault();
                // Add our ConnectionStatusChanged event handler.
                _imuService.Device.ConnectionStatusChanged += ConnectionStatusChanged;

                
            }
            catch (Exception exception)
            {
                // Trace.
                Trace("Unable to get device services and characteristics for " + deviceName);

                // Throw IntelligentHeadsetSensorPeripheralException.
                throw new IntelligentHeadsetSensorPeripheralException(
                    IntelligentHeadsetSensorPeripheralException.ErrorCode.AttachFailed,
                    "Unable to get device services and characteristics for " + deviceName,
                    exception);
            }
            
            // We're attached.
            Attached = true;

            // Trace.
            Trace("Successfully attached to " + deviceName);

            // Process the connection status.
            await ProcessConnectionStatus();
        }

        /// <summary>
        /// Detaches the Intelligent Headset device.
        /// </summary>
        /// <remarks>This metod should not be called from multiple threads.</remarks>
        public void Detach()
       {
            // Get the device name.
            var deviceName = IntelligentHeadsetSensorDevice.DeviceInformation.Name;

            // If we're already detached, throw an exception.
            if (!Attached)
            {
                // Trace.
                Trace("Already detached from " + deviceName);

                // Throw exception.
                throw new IntelligentHeadsetSensorPeripheralException(
                    IntelligentHeadsetSensorPeripheralException.ErrorCode.AlreadyDetached,
                    "Already detached from " + deviceName,
                    null);
            }

            // Unsubscribe from characteristics.
            UnsubscribeToCharacteristics();

            // Stop listening for the ConnectionStatusChanged event.
            _imuService.Device.ConnectionStatusChanged -= ConnectionStatusChanged;

            // Dispose of state.
            _imuService = null;
            _systemService = null;
            _gpsService = null;
            _deviceInfoService = null;
            _batteryInfoService = null;
            _magnetometerCalibrationCharacteristic = null;
            _magnetometerCharacteristic = null;
            _comboHprCharacteristic = null;
            _accelerometerCharacteristic = null;
            _gpsAllInOneCharacteristic = null;

            // If the connected state is true, set it to false.
            if (Connected)
            {
                Connected = false;
                OnConnectionChanged(EventArgs.Empty);
            }

            // Clear the attached state.
            Attached = false;
        }

        /// <summary>
        /// Raises the ConnectionFailed event.
        /// </summary>
        /// <param name="connectionFailedEventArgs">An ConnectionFailedEventArgs that contains the event data.</param>
        protected virtual void OnConnectionFailed(ConnectionFailedEventArgs connectionFailedEventArgs)
        {
            // Event handler check.
            var connectionFailed = ConnectionFailed;
            if (connectionFailed != null)
            {
                // Trace.
                Trace("Raising ConnectionFailed");

                // Raise the event.
                connectionFailed(this, connectionFailedEventArgs);
            }
        }

        /// <summary>
        /// Raises the ConnectionChanged event.
        /// </summary>
        /// <param name="eventArgs">An EventArgs that contains the event data.</param>
        protected virtual void OnConnectionChanged(EventArgs eventArgs)
        {
            // Event handler check.
            var connectionChanged = ConnectionChanged;
            if (connectionChanged != null)
            {
                // Trace.
                Trace("Raising ConnectionChanged " + Connected);

                // Raise the event.
                connectionChanged(this, eventArgs);
            }
        }

        /// <summary>
        /// Raises the CompassChanged event.
        /// </summary>
        /// <param name="compassChangedEventArgs">The CompassChangedEventArgs that contains the event data.</param>
        protected virtual void OnCompassChanged(CompassChangedEventArgs compassChangedEventArgs)
        {
            // Event handler check.
            var compassChanged = CompassChanged;
            if (compassChanged != null)
            {
                // Trace.
                Trace("Raising CompassChanged " + compassChangedEventArgs.CompassHeading);

                // Raise the event.
                compassChanged(this, compassChangedEventArgs);
            }
        }
    
        protected virtual void OnGyroChanged(GyroChangedEventArgs args)
        {
            // Event handler check.
            var gyroChanged = GyroChanged;
            if (gyroChanged != null)
            {
                // Trace.
                Trace("Raising GryroChanged" );

                // Raise the event.
                gyroChanged(this, args);
            }
        }

        /// <summary>
        /// Raises the GeocoordinateChanged event.
        /// </summary>
        /// <param name="geocoordinateChangedEventArgs">The GeocoordinateChangedEventArgs that contains the event data.</param>
        protected virtual void OnGeocoordinateChanged(GeocoordinateChangedEventArgs geocoordinateChangedEventArgs)
        {
            // Event handler check.
            var geocoordinateChanged = GeocoordinateChanged;
            if (geocoordinateChanged != null)
            {
                // Trace.
                Trace("Raising GeocoordinateChanged. Latitude: " +
                    geocoordinateChangedEventArgs.SensorGeocoordinate.Latitude.ToString("0.00000") +
                    " Longitude: " +
                    geocoordinateChangedEventArgs.SensorGeocoordinate.Longitude.ToString("0.00000"));

                // Raise the event.
                geocoordinateChanged(this, geocoordinateChangedEventArgs);
            }
        }

        /// <summary>
        /// Calculates an authentication response (key) based on the given nonce value.
        /// Example: Received nonce: 0x2bd45fbb, authentication key: 0x7e7d3bd2
        /// </summary>
        /// <param name="nonce">The nonce value.</param>
        /// <returns>The calculated authentication response (key).</returns>
        private static uint CalculateAuthenticationKey(uint nonce)
        {
            ulong temp = (nonce ^ SharedSecret);
            int lowerFiveBitsOfNonce = (int)nonce & 0x1F;

            // Rotate left by the amount defined by the lower 5 bits of the nonce
            temp = LeftRotate(temp, (int)nonce & 0x1F);
            return (uint)temp;
        }

        /// <summary>
        /// Performs a left rotation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="step">The step.</param>
        /// <returns>The left-rotated value.</returns>
        private static ulong LeftRotate(ulong value, int step)
        {
            return (value << step) | (value >> (32 - step));
        }

        /// <summary>
        /// Authenticates and subscribes to characteristics.
        /// </summary>
        private async Task AuthenticateAndSubscribeToCharacteristicsAsync()
        {
            // Trace.
            Trace("Attempting to authenticate and subscribe to characteristics.");

            // Try to authenticate and subscribe to characteristics.
            try
            {
                // Get the nonce characteristic.
                Guid nonceGuid = Guid.Parse(Identifiers.Nonce);
                var nonceCharacteristic = _systemService.GetAllCharacteristics()?.Where(c => c.Uuid == nonceGuid).FirstOrDefault();
                if (nonceCharacteristic == null)
                {
                    // Trace.
                    Trace("Unable to authenticate and subscribe to characteristics. Unable to get NONCE characteristic.");

                    // Throw exception.
                    throw new IntelligentHeadsetSensorPeripheralException(IntelligentHeadsetSensorPeripheralException.ErrorCode.AuthenticateSubscribeFailed,
                        "Unable to authenticate and subscribe to characteristics. Unable to get NONCE characteristic.",
                        null);
                }

                // Read the nonce characteristic value.
                uint nonce;
                GattReadResult readResult = await nonceCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                if (readResult.Status == GattCommunicationStatus.Success && readResult.Value.Length != 0)
                {
                    byte[] data = WindowsRuntimeBufferExtensions.ToArray(readResult.Value);
                    nonce = BitConverter.ToUInt32(data, 0);
                }
                else
                {
                    // Trace.
                    Trace("Unable to authenticate and subscribe to characteristics. Unable to read NONCE characteristic.");

                    // Throw exception.
                    throw new IntelligentHeadsetSensorPeripheralException(IntelligentHeadsetSensorPeripheralException.ErrorCode.AuthenticateSubscribeFailed,
                        "Unable to authenticate and subscribe to characteristics. Unable to read NONCE characteristic.",
                        null);
                }

                // Read the access key characteristic.
                var characteristic = _systemService.GetCharacteristics(Guid.Parse(Identifiers.Key))?.FirstOrDefault();
                if (characteristic == null)
                {
                    // Trace.
                    Trace("Unable to authenticate and subscribe to characteristics. Unable to get IDENTIFIER characteristic.");

                    // Throw exception.
                    throw new IntelligentHeadsetSensorPeripheralException(IntelligentHeadsetSensorPeripheralException.ErrorCode.AuthenticateSubscribeFailed,
                        "Unable to authenticate and subscribe to characteristics. Unable to get IDENTIFIER characteristic.",
                        null);
                }

                // Authenticate.
                uint authKey = CalculateAuthenticationKey(nonce);
                byte[] authKeyAsByteArray = BitConverter.GetBytes(authKey);
                await characteristic.WriteValueAsync(authKeyAsByteArray.AsBuffer(), GattWriteOption.WriteWithResponse);

                // Subscribe to the ValueChanged event for the ComboHPR characteristic.
                _comboHprCharacteristic.ValueChanged += ComboHprCharacteristicValueChanged;
                await _comboHprCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

                _accelerometerCharacteristic.ValueChanged += Accelerometer_ValueChanged;
                await _accelerometerCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

                // Subscribe to the ValueChanged event for AllInOne characteristic.
                _gpsAllInOneCharacteristic.ValueChanged += AllInOneCharacteristicValueChanged;
                await _gpsAllInOneCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                
            }
            catch (IntelligentHeadsetSensorPeripheralException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Trace("Unable to authenticate and subscribe to characteristics. Exception was: " + exception.Message);
                throw new IntelligentHeadsetSensorPeripheralException(IntelligentHeadsetSensorPeripheralException.ErrorCode.AuthenticateSubscribeFailed,
                    "Unable to authenticate and subscribe to characteristics.",
                    exception);
            }

            // Trace.
            Trace("Succesfully authenticated and subscribed to characteristics.");
        }

        protected virtual void OnAccelerometerValueChanged(AccelerometerValueChangedEventArgs args)
        {
            if (AccelerometerValueChanged != null)
                AccelerometerValueChanged(this, args);
          }
        private void Accelerometer_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            IBuffer buffer = args.CharacteristicValue;
            
            //System.Diagnostics.Debug.WriteLine("Buffer Length:" + buffer.Length);

            // The desired buffer length.
            const uint desiredBufferLength = 6;

            // Make sure we have a characteristic value.
            if (args.CharacteristicValue == null || args.CharacteristicValue.Length < desiredBufferLength)
            {
                return;
            }

            // Note that we're grabbing more values than we're using right now in the code below because
            // the code existed, and we'll use them eventually, so leaving it in place makes the most sense.
            byte[] data = args.CharacteristicValue.ToArray();
            if (data != null && data.Length >= desiredBufferLength)
            {
                // Obtain the raw data values.
                Int16 xRaw = BitConverter.ToInt16(data, 0);
                Int16 yRaw = BitConverter.ToInt16(data, 2);
                Int16 zRaw = BitConverter.ToInt16(data, 4);

                double maxValueOfInt16 = Int16.MaxValue;

                //float accelerationX = (float)Math.Min(Math.Max((xRaw + maxValueOfInt16 / 2) / maxValueOfInt16, 0.0d), 1.0d);
                //float accelerationY = (float)Math.Min(Math.Max((yRaw + maxValueOfInt16 / 2) / maxValueOfInt16, 0.0d), 1.0d);
                //float accelerationZ = (float)Math.Min(Math.Max((zRaw + maxValueOfInt16 / 2) / maxValueOfInt16, 0.0d), 1.0d);

                float accelerationX = ((float)xRaw)/ 16384f;
                float accelerationY = ((float)yRaw) / 16384f;
                float accelerationZ = ((float)zRaw) / 16384f;

                AccelerometerReading reading = new AccelerometerReading(accelerationX, accelerationY, accelerationZ);
                this.currentReading.CurrentAccelerometerReading = reading;
                OnAccelerometerValueChanged(new AccelerometerValueChangedEventArgs(currentReading.CurrentAccelerometerReading, currentReading.PreviousAccelerometerReading));
                ////System.Diagnostics.Debug.WriteLine("x: {0}, Y:{1}, Z{2}", accelerationX, accelerationY, accelerationZ);
            }
        }

        /// <summary>
        /// Unsubscribes to characteristics from the Intelligent Headset device.
        /// </summary>
        private void UnsubscribeToCharacteristics()
        {
            // Trace.
            Trace("Attempting to unsubscribe to characteristics.");

            // Unsubscribe to the ValueChanged event for the ComboHPR characteristic.
            _comboHprCharacteristic.ValueChanged -= ComboHprCharacteristicValueChanged;

            // Unsubscribe to the ValueChanged event for AllInOne characteristic.
            _gpsAllInOneCharacteristic.ValueChanged -= AllInOneCharacteristicValueChanged;

            // Trace.
            Trace("Succesfully unsubscribed to characteristics.");
        }

        /// <summary>
        /// Indicates whether this devices is enabled.
        /// </summary>
        //public async Task<bool> IsEnabled ()
        //{
        //        foreach (string key in IntelligentHeadsetSensorDevice.DeviceInformation.Properties.Keys)
        //            //System.Diagnostics.Debug.WriteLine(key);
        //        //DeviceInformation.CreateFromIdAsync (IntelligentHeadsetSensorDevice.DeviceInformation.)
        //    string s = IntelligentHeadsetSensorDevice.DeviceInformation.Properties["System.Devices.DeviceInstanceId"].ToString();
        //    DeviceInformation device = await DeviceInformation.CreateFromIdAsync(s, null, DeviceInformationKind.Device);

        //    ////System.Diagnostics.Debug.WriteLine("Is Connected:{ 0}", device.IsEnabled);


        //    return IntelligentHeadsetSensorDevice.DeviceInformation.IsEnabled;
        //}
        /// <summary>
        /// Processes the connection status.
        /// </summary>
        /// <returns>The task.</returns>
        private async Task ProcessConnectionStatus()
        {
            // Get the connection status.
            bool connected = _imuService.Device.ConnectionStatus == BluetoothConnectionStatus.Connected;

            // Trace.
            Trace("Connection status " + _imuService.Device.ConnectionStatus);

            // Process the event.
            if (connected != Connected)
            {
                // Set the new connected state.
                Connected = connected;

                // If we're attached, process the event.
                if (Attached)
                {
                    // When we're connected, authenticate and subscribe to characteristics. When we're not
                    // connected, unsubscribe to characteristics.
                    if (Connected)
                    {
                        await AuthenticateAndSubscribeToCharacteristicsAsync();
                    }
                    else
                    {
                        UnsubscribeToCharacteristics();
                    }

                    // Raise the ConnectionChanged event.
                    if (Attached)
                    {
                        OnConnectionChanged(EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Connection status changed event handler.
        /// </summary>
        /// <param name="device">The source of the event.</param>
        /// <param name="args">A BluetoothLEDevice that contains the event data.</param>
        private async void ConnectionStatusChanged(BluetoothLEDevice device, object args)
        {
            try
            {
                _imuService.Device.ConnectionStatusChanged -= ConnectionStatusChanged;
                await ProcessConnectionStatus();
            }
            catch (Exception exception)
            {
                OnConnectionFailed(new ConnectionFailedEventArgs(exception));
            }
        }

        /// <summary>
        /// ComboHPR value changed event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">A GattValueChangedEventArgs that contains the event data.</param>
        private void ComboHprCharacteristicValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {           
            lock (syncRoot)
            {
                // The desired buffer length.
                const uint desiredBufferLength = 14;

                // Make sure we have a characteristic value.
                if (args.CharacteristicValue == null || args.CharacteristicValue.Length < desiredBufferLength)
                {
                    return;
                }

                // Note that we're grabbing more values than we're using right now in the code below because
                // the code existed, and we'll use them eventually, so leaving it in place makes the most sense.
                byte[] data = args.CharacteristicValue.ToArray();
                if (data != null && data.Length >= desiredBufferLength)
                {
                    // Obtain the raw data values.
                    ushort fusedHeadingRaw = BitConverter.ToUInt16(data, 0);
                    Int16 pitchRaw = BitConverter.ToInt16(data, 2);
                    Int16 rollRaw = BitConverter.ToInt16(data, 4);
                    byte paceRaw = data[6];
                    byte flagsRaw = data[7];

                    ComboHprFlags flags = (ComboHprFlags)Enum.Parse(typeof(ComboHprFlags), flagsRaw.ToString());
                    //Int16 flagsRaw = BitConverter.ToInt16(data, 7);

                    if ((flags & ComboHprFlags.MageneticDesturbance) != 0)
                        System.Diagnostics.Debug.WriteLine("ComboHPRFlags.MageneticDesturbance");

                    if ((flags & ComboHprFlags.AutoCalibrated) != 0)
                        System.Diagnostics.Debug.WriteLine("ComboHPRFlags.AutoCalibrated");


                    //bool magneticDisturbance = flagsRaw & 0x02;

                    ushort fieldStrengthRaw = BitConverter.ToUInt16(data, 8);
                    ushort yawRaw = BitConverter.ToUInt16(data, 10);
                    ushort cheadingRaw = BitConverter.ToUInt16(data, 12);

                    // Obtain the finished values.
                    double fusedHeading = (double)fusedHeadingRaw / 10.0;
                    double pitch = (double)pitchRaw / 10.0;
                    double roll = (double)rollRaw / 10.0;
                    double yaw = (double)yawRaw / 10.0;
                    double compassHeading = (double)cheadingRaw / 10.0;

                    // Raise the CompassChanged and GyroChanged event.
                    if (Attached && Connected)
                    {
                        ComboHprReading comboHprReading = new ComboHprReading(fusedHeading - yawOffset, pitch - pitchOffset, roll - rollOffset, compassHeading, flags, args.Timestamp);
                        currentRawComboHprReading = new ComboHprReading(fusedHeading, pitch, roll, compassHeading, flags, args.Timestamp);
                        currentReading.CurrentComboHprReading = comboHprReading;

                        //SensorGeocoordinate gps = new SensorGeocoordinate()

                        //OnGeocoordinateChanged(new GeocoordinateChangedEventArgs( )
                        OnCompassChanged(new CompassChangedEventArgs(compassHeading, args.Timestamp));
                        OnGyroChanged(new GyroChangedEventArgs(comboHprReading.Yaw, comboHprReading.Pitch, comboHprReading.Roll, comboHprReading.CompassHeading, flags, comboHprReading.TimeStamp));

                        //System.Diagnostics.Debug.WriteLine("Fushed Heading {0}", fusedHeading);
                    }
                }
            }
        }

        private object syncRoot = new object();
        private CurrentIntelligentHeadsetReading currentReading = new CurrentIntelligentHeadsetReading();
        private ComboHprReading currentRawComboHprReading = null;
        public async Task<CurrentIntelligentHeadsetReading> GetCurrentReadingAsync ()
        {
            //while (currentReading == null)
            //{
            //    if (!Connected)
            //        throw new Exception("Not Connected");

            //    await Task.Delay(new TimeSpan(0, 0, 1));
            //}

            return currentReading;
        }

        private double yawOffset = 0;
        private double pitchOffset = 0;
        private double rollOffset = 0;


        public async Task Calibrate ()
        {
            int idx = 0;
            while (currentRawComboHprReading == null && idx < 5)
            {
                idx++;
                await Task.Delay(new TimeSpan(500));
            }
            lock (syncRoot)
            {
                if (currentRawComboHprReading != null)
                {
                    yawOffset = currentRawComboHprReading.Yaw;
                    pitchOffset = currentRawComboHprReading.Pitch;
                    rollOffset = currentRawComboHprReading.Roll;
                }
            }
        }

     
        /// <summary>
        /// AllInOne value changed event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">A GattValueChangedEventArgs that contains the event data.</param>
        private void AllInOneCharacteristicValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            // The desired buffer length.
            const uint desiredBufferLength = 16;

            // Make sure we have a characteristic value.
            if (args.CharacteristicValue == null || args.CharacteristicValue.Length < desiredBufferLength)
            {
                return;
            }

            byte[] data = args.CharacteristicValue.ToArray();
            if (data != null && data.Length >= desiredBufferLength)
            {
                Int32 latitude = BitConverter.ToInt32(data, 0);
                Int32 longitude = BitConverter.ToInt32(data, 4);
                UInt32 timeAltFix = BitConverter.ToUInt32(data, 8);
                UInt32 speedCourseHorizerror = BitConverter.ToUInt32(data, 12);

                // timeAltFix content:
                // bit(0‐16): seconds sincemidnight, bit17‐28: altitude(0..4095m), bit29‐31: fixtype(0..7)
                UInt32 secondsSincemidnight = timeAltFix & 0x1FFFF;
                UInt32 altitude = (timeAltFix >> 17) & 0x7FFF;
                UInt32 fixtype = (timeAltFix >> 29) & 0x7;

                // speedCourseHorizerror content:
                // bit 0‐10: speed (x0.1km/t) ‐ 0‐204.7,  bit 11‐19: course (0‐359), bit 20‐25: Horizontal error (0‐31.5m, LSB=0.5m), 
                // bit 26‐30: satellite count (0..31),  bit 31: data valid
                UInt32 speed = speedCourseHorizerror & 0x3FF;
                UInt32 course = (speedCourseHorizerror >> 11) & 0x1FF;
                UInt32 horizontalErrorLSB = (speedCourseHorizerror >> 20) & 0x1;
                UInt32 horizontalError = (speedCourseHorizerror >> 21) & 0x1F;
                UInt32 satelliteCount = (speedCourseHorizerror >> 26) & 0x1F;
                UInt32 dataValid = (speedCourseHorizerror >> 31) & 0x1;

                double finishedLatitude = (double)latitude / 600000.0;
                double finishedLongitude = (double)longitude / 600000.0;
                double horizontalAccuracy = (double)horizontalError + (horizontalErrorLSB == 1 ? 0.5 : 0);
                bool valid = dataValid == 1;

                //_gpsReading.Latitude = ;
                //_gpsReading.Longitude = (double)longitude / 600000;
                //_gpsReading.Altitude = altitude;
                //_gpsReading.HorizontalAccuracy = (double)horizontalError + (horizontalErrorLSB == 1 ? 0.5 : 0);

                // Raise the GeocoordinateChanged event.
                if (Attached && Connected)
                {
                    OnGeocoordinateChanged(new GeocoordinateChangedEventArgs(new SensorGeocoordinate(finishedLatitude, finishedLongitude, altitude, horizontalAccuracy, valid)));
                }
            }
        }

        // Trace output.
        private void Trace(String text)
        {
            //System.Diagnostics.Debug.WriteLine("IntelligentHeadsetSensorPeripheral: " + text);
        }
    }
}
