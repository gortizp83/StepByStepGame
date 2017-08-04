//
// IntelligentHeadsetSensorDevice.cs
//
// Copyright © 2015 Microsoft Corporation. All rights reserved.
// Created by blambert, October 2015. 
// 

using Windows.Devices.Enumeration;

namespace Microsoft.PartnerCatalyst.IntelligentHeadset
{
    // IntelligentHeadsetSensorDevice class.
    public class IntelligentHeadsetSensorDevice
    {
        // Gets the device information for the intelligent headset device.
        public DeviceInformation DeviceInformation { get; private set; }

        /// <summary>
        /// Initializes a new IntelligentHeadsetSensorDevice.
        /// </summary>
        /// <param name="deviceInformation">The DeviceInformation of the Intelligent Headset device.</param>
        public IntelligentHeadsetSensorDevice(DeviceInformation deviceInformation)
        {
            // Initialize.
            DeviceInformation = deviceInformation;
        }
    }
}
