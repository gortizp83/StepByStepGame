//
// SensorGeocoordinate.cs
//
// Copyright © 2015 Microsoft Corporation. All rights reserved.
// Created by blambert, October 2015. 
//

using System;

namespace Microsoft.PartnerCatalyst.NavigationSensorPeripheral
{
    /// <summary>
    /// SensorGeocoordinate class. Represents a sensor geocoordinate.
    /// </summary>
    public class SensorGeocoordinate
    {
        /// <summary>
        /// Gets the latitude.
        /// </summary>
        public DateTime Timestamp { get; } = DateTime.Now;

        /// <summary>
        /// Gets the latitude.
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// Gets the longitude.
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Gets the altitude.
        /// </summary>
        public double Altitude { get; }

        /// <summary>
        /// Gets the horizontal accuracy.
        /// </summary>
        public double HorizontalAccuracy { get; }

        /// <summary>
        /// Gets a value which indic
        /// </summary>
        public bool Valid { get; }

        /// <summary>
        /// Initializes a new instance of the SensorGeocoordinate class.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="altitude">The altitide.</param>
        /// <param name="horizontalAccuracy">The horizontal accuracy, expressed in meters.</param>
        /// <param name="valid">A value which indicates whether the sensor coordinate is valid.</param>
        public SensorGeocoordinate(double latitude, double longitude, double altitude, double horizontalAccuracy, bool valid)
        {
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
            HorizontalAccuracy = horizontalAccuracy;
            Valid = valid;
        }
    }
}
