//
// GeocoordinateChangedEventArgs.cs
//
// Copyright © 2015 Microsoft Corporation. All rights reserved.
// Created by blambert, October 2015. 
// 

using System;

namespace Microsoft.PartnerCatalyst.NavigationSensorPeripheral
{
    // GeocoordinateChangedEventArgs class.
    public class GeocoordinateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the sensor geocoordinate.
        /// </summary>
        public SensorGeocoordinate SensorGeocoordinate { get; }

        /// <summary>
        /// Initializes a new instance of the GeocoordinateChangedEventArgs class.
        /// </summary>
        /// <param name="sensorGeocoordinate">A SensorGeocoordinate representing the sensor geocoordinate. </param>
        public GeocoordinateChangedEventArgs(SensorGeocoordinate sensorGeocoordinate)
        {
            SensorGeocoordinate = sensorGeocoordinate;
        }
    }
}
