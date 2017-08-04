//
// INavigationSensorPeripheral.cs
//
// Copyright © 2015 Microsoft Corporation. All rights reserved.
// Created by blambert, October 2015. 
//

using System;
using Windows.Foundation;

namespace Microsoft.PartnerCatalyst.NavigationSensorPeripheral
{
    /// <summary>
    /// Abstract interface to a navigation sensor peripheral.
    /// </summary>
    public interface INavigationSensorPeripheral
    {
        /// <summary>
        /// ConnectionChanged event.
        /// </summary>
        event TypedEventHandler<INavigationSensorPeripheral, EventArgs> ConnectionChanged;

        /// <summary>
        /// GeocoordinateChanged event.
        /// </summary>
        event TypedEventHandler<INavigationSensorPeripheral, GeocoordinateChangedEventArgs> GeocoordinateChanged;

        /// <summary>
        /// CompassChanged event.
        /// </summary>
        event TypedEventHandler<INavigationSensorPeripheral, CompassChangedEventArgs> CompassChanged;

        /// <summary>
        /// Gets a value which indicates whether we're connected to the Intelligent Headset device.
        /// </summary>
        bool Connected { get; }
    }
}
