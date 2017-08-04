//
// IntelligentHeadsetSensorPeripheralException.cs
//
// Copyright © 2015 Microsoft Corporation. All rights reserved.
// Created by blambert, October 2015. 
// 

using System;

namespace Microsoft.PartnerCatalyst.IntelligentHeadset
{
    /// <summary>
    /// IntelligentHeadsetSensorPeripheralException class.
    /// </summary>
    public class IntelligentHeadsetSensorPeripheralException : Exception
    {
        /// <summary>
        /// ErrorCode enumeration.
        /// </summary>
        public enum ErrorCode
        {
            AlreadyAttached,
            AlreadyDetached,
            AttachFailed,
            AuthenticateSubscribeFailed,
            UnsubscribeFailed
        }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        public ErrorCode Error { get; }

        /// <summary>
        /// Initializes a new instance of the IntelligentHeadsetSensorPeripheralException.
        /// </summary>
        /// <param name="error">The error code.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public IntelligentHeadsetSensorPeripheralException(ErrorCode error, string message, Exception innerException)
            : base(message, innerException)
        {
            Error = error;
        }
    }
}
