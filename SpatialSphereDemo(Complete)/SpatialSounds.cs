using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Storage;

namespace SpatialSphereDemo
{
    /// <summary>
    /// Represents a spatial audio sound.
    /// </summary>
    public class SpatialSoundSource
    {
        public SpatialSoundSource() { }

        /// <summary>
        /// Creates a SpatialSoundSource insance.
        /// </summary>
        /// <param name="position">Position of the spatial sound.</param>
        /// <param name="soundFileResourceLocation">The resource location of the spatial sound file.</param>
        /// <param name="gain">The amount of gain to be applied to the spatial sound.</param>
        /// <param name="imageResourceLocation">lcation of the image resource for the spatial sound.</param>
        public SpatialSoundSource(Vector3 position, string soundFileResourceLocation, double gain, Point imageResourceLocation)
        {
            this.Position = position;
            this.SoundFileResourceLocation = soundFileResourceLocation;
            this.Gain = gain;
            this.ImageResourceLocation = imageResourceLocation;
        }

        /// <summary>
        /// The Position of the spatial audio
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// The amount of gain that will be applied to the SoundFile.
        /// </summary>
        public double Gain { get; set; }

        /// <summary>
        /// Resource location for the image use to present this sound.
        /// </summary>
        public Point ImageResourceLocation { get; set; }

        /// <summary>
        /// Gets and Sets the na
        /// </summary>
        public string SoundFileResourceLocation { get; set; }

        /// <summary>
        /// Gets the StorageFile for spatial audio.
        /// </summary>
        /// <returns>The 48Kz, Mono StorageFile for Spatial Audio</returns>
        public async Task<StorageFile> GetSoundFileAsync()
        {
            ResourceContext rc = ResourceContext.GetForCurrentView();
            var resource = ResourceManager.Current.MainResourceMap.GetValue(SoundFileResourceLocation, rc);
            StorageFile file = await resource.GetValueAsFileAsync();
            return file;
        }

        public async static Task<StorageFile> GetMonoSoundFile()
        {
            ResourceContext rc = ResourceContext.GetForCurrentView();
            var resource = ResourceManager.Current.MainResourceMap.GetValue("Files/Audio/MonoSound.wav", rc);
            StorageFile file = await resource.GetValueAsFileAsync();
            return file;
        }
    }
}
