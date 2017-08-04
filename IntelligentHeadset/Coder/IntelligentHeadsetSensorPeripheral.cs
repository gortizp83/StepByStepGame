//
// IntelligentHeadsetSensorPeripheral.cs
//
// Copyright © 2015 Microsoft Corporation. All rights reserved.
// Created by blambert, October 2015. 
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Microsoft.PartnerCatalyst.NavigationSensorPeripheral;

namespace Microsoft.PartnerCatalyst.IntelligentHeadset
{
    // IntelligentHeadsetSensorPeripheral class.
    public class IntelligentHeadsetSensorPeripheral : INavigationSensorPeripheral
    {
        // GeocoordinateChanged event.
        public event TypedEventHandler<INavigationSensorPeripheral, GeocoordinateChangedEventArgs> GeocoordinateChanged;

        // CompassChanged event.
        public event TypedEventHandler<INavigationSensorPeripheral, CompassChangedEventArgs> CompassChanged;
    }
}
