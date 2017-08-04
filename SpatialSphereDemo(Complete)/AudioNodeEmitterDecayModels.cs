using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Audio;

namespace SpatialSphereDemo
{
    /// <summary>
    /// Helper class to add defaults for the AudioNodeEmitterDecayModel class
    /// </summary>
    public static class AudioNodeEmitterDecayModels
    {
        /// <summary>
        /// Returns a new AudioNodeEmitterDecayModel with Natural decay.
        /// Same defaults as XAudio2
        /// </summary>
        /// <returns></returns>
        public static AudioNodeEmitterDecayModel CreateNatural()
        {
            return AudioNodeEmitterDecayModel.CreateNatural(1.5848931924e-5, 3.98d, 1, float.MaxValue);
        } 
    }
}
