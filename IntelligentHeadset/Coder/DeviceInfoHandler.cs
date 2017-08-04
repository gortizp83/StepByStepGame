using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Microsoft.PartnerCatalyst.IntelligentHeadset
{
    class IHSDeviceInfoHandler
    {
        private GattDeviceService _deviceInfoService;
        private GattDeviceService _batteryInfoService;
        private DeviceInfo _deviceInfo;

        public IHSDeviceInfoHandler(
            IHSDeviceInfo deviceInfo,
            GattDeviceService deviceInfoService,
            GattDeviceService batteryInfoService)
        {
            _deviceInfo = deviceInfo;
            _deviceInfoService = deviceInfoService;
            _batteryInfoService = batteryInfoService;
        }

        /// <summary>
        /// Resolves the device information.
        /// </summary>
        public async Task ReadDeviceInformationAsync()
        {
            if (_deviceInfoService != null)
            {
                IReadOnlyList<GattCharacteristic> characteristicList =
                    _deviceInfoService.GetAllCharacteristics();

                foreach (var characteristic in characteristicList)
                {
                    Guid uuid = characteristic.Uuid;
                    GattReadResult readResult = await characteristic.ReadValueAsync();
                    byte[] data = WindowsRuntimeBufferExtensions.ToArray(readResult.Value);

                    if (uuid == GattCharacteristic.ConvertShortIdToUuid(IHSUuids.SystemId))
                    {
                        // Not implemented
                    }
                    else if (uuid == GattCharacteristic.ConvertShortIdToUuid(IHSUuids.ModelNumber))
                    {
                        _deviceInfo.ModelNumber = Encoding.UTF8.GetString(data);
                    }
                    else if (uuid == GattCharacteristic.ConvertShortIdToUuid(IHSUuids.SerialNumber))
                    {
                        _deviceInfo.SerialNumber = Encoding.UTF8.GetString(data);
                    }
                    else if (uuid == GattCharacteristic.ConvertShortIdToUuid(IHSUuids.FirmwareRevision))
                    {
                        _deviceInfo.FirmwareRevision = Encoding.UTF8.GetString(data);
                    }
                    else if (uuid == GattCharacteristic.ConvertShortIdToUuid(IHSUuids.HardwareRevision))
                    {
                        _deviceInfo.HardwareRevision = Encoding.UTF8.GetString(data);
                    }
                    else if (uuid == GattCharacteristic.ConvertShortIdToUuid(IHSUuids.SoftwareRevision))
                    {
                        _deviceInfo.SoftwareRevision = Encoding.UTF8.GetString(data);
                    }
                }
            }
        }

        /// <summary>
        /// Resolves the battery information.
        /// </summary>
        public async Task ReadDeviceBatteryInformationAsync()
        {
            if (_batteryInfoService != null)
            {
                IReadOnlyList<GattCharacteristic> characteristicList = _batteryInfoService.GetAllCharacteristics();

                foreach (var characteristic in characteristicList)
                {
                    if (characteristic.Uuid == GattCharacteristic.ConvertShortIdToUuid(IHSUuids.BatteryLevel))
                    {
                        GattReadResult readResult = await characteristic.ReadValueAsync();
                        byte[] data = WindowsRuntimeBufferExtensions.ToArray(readResult.Value);
                        // TODO: Store value
                    }
                }
            }
        }
    }
}
