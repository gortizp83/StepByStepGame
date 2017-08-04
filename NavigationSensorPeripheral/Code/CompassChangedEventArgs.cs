//
// CompassChangedEventArgs.cs
//
// Copyright © 2015 Microsoft Corporation. All rights reserved.
// Created by blambert, October 2015. 
// 

using System;

namespace Microsoft.PartnerCatalyst.NavigationSensorPeripheral
{
    // CompassChangedEventArgs class.
    public class CompassChangedEventArgs
    {
        // Gets the compass heading.
        public double CompassHeading { get; }

        public DateTimeOffset Timespan { get; }

        /// <summary>
        /// CompassChangedEventArgs constructor.
        /// </summary>
        /// <param name="compassHeading">The compass heading in degrees.</param>
        public CompassChangedEventArgs(double compassHeading, DateTimeOffset timespan)
        {
            CompassHeading = compassHeading;
        }
    }
}
