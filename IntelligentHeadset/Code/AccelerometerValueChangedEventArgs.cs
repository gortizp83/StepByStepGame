using Microsoft.PartnerCatalyst.NavigationSensorPeripheral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PartnerCatalyst.IntelligentHeadset
{
    public class AccelerometerValueChangedEventArgs
    {
        public AccelerometerReading PreviousAccelerometerReading { get; set; }
        public AccelerometerReading CurrentAccelerometerReading { get; set; }
        public AccelerometerValueChangedEventArgs (AccelerometerReading currentAccelerometerReading, AccelerometerReading previousAccelerometerReading)
        {
            this.PreviousAccelerometerReading = previousAccelerometerReading;
            this.CurrentAccelerometerReading = currentAccelerometerReading;
        }
    }
}
