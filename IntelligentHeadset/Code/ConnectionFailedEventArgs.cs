//
// ConnectionFailedEventArgs.cs
//
// Copyright © 2015 Microsoft Corporation. All rights reserved.
// Created by blambert, October 2015. 
// 

using System;

namespace Microsoft.PartnerCatalyst.IntelligentHeadset
{
    /// <summary>
    /// ConnectionFailedEventArgs class.
    /// </summary>
    public class ConnectionFailedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the exception. 
        /// </summary>
        public Exception Exception { get; }

        public ConnectionFailedEventArgs(Exception exception)
        {
            Exception = exception;

        }
    }
}
