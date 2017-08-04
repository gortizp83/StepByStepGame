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
        public enum Note
        {
            Mono,
            A,
            As,
            B,
            C,
            Cs,
            D,
            Ds,
            E,
            F,
            Fs,
            G,
            Gs,
        };

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

        public async static Task<StorageFile> GetMonoSoundFile(Note note)
        {
            ResourceContext rc = ResourceContext.GetForCurrentView();
            //var resource = ResourceManager.Current.MainResourceMap.GetValue("Files/Audio/MonoSound.wav", rc);
            var resource = ResourceManager.Current.MainResourceMap.GetValue("Files/Audio/piano-a.wav", rc);
            //switch (note)
            //{
            //    case Note.A:
            //        resource = ResourceManager.Current.MainResourceMap.GetValue("Files/Audio/piano-a.wav", rc);
            //        break;
            //    case Note.As:
            //        resource = ResourceManager.Current.MainResourceMap.GetValue("Files/Audio/piano-as.wav", rc);D:\git\SpatialDemo\SpatialSphereDemo(Complete)\Audio\piano-g.wav
            //        break;
            //    case Note.B:
            //        resource = ResourceManager.Current.MainResourceMap.GetValue("Files/Audio/piano-b.wav", rc);
            //        break;
            //    case Note.C:
            //        resource = ResourceManager.Current.MainResourceMap.GetValue("Files/Audio/piano-c.wav", rc);
            //        break;
            //    case Note.Cs:
            //        resource = ResourceManager.Current.MainResourceMap.GetValue("Files/Audio/piano-cs.wav", rc);
            //        break;
            //    case Note.D:
            //        resource = ResourceManager.Current.MainResourceMap.GetValue("Files/Audio/piano-d.wav", rc);
            //        break;
            //    case Note.Ds:
            //        resource = ResourceManager.Current.MainResourceMap.GetValue("Files/Audio/piano-ds.wav", rc);
            //        break;
            //    case Note.E:
            //        resource = ResourceManager.Current.MainResourceMap.GetValue("Files/Audio/piano-e.wav", rc);
            //        break;
            //    case Note.F:
            //        resource = ResourceManager.Current.MainResourceMap.GetValue("Files/Audio/piano-f.wav", rc);
            //        break;
            //    case Note.G:
            //        resource = ResourceManager.Current.MainResourceMap.GetValue("Files/Audio/piano-g.wav", rc);
            //        break;
            //    case Note.Gs:
            //        resource = ResourceManager.Current.MainResourceMap.GetValue("Files/Audio/piano-gs.wav", rc);
            //        break;
            //}

            StorageFile file = await resource.GetValueAsFileAsync();
            return file;
        }
    }
}
