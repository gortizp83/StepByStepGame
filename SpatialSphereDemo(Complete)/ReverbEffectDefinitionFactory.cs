using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Audio;

namespace SpatialSphereDemo
{
    public static class ReverbEffectDefinitionFactory
    {
        public static ReverbEffectDefinition GetReverbEffectDefinition (AudioGraph graph, ReverbEffectDefinitions reverbEffect)
        {
            ReverbEffectDefinition effect = null;
            switch (reverbEffect)
            {
                case ReverbEffectDefinitions.SmallRoom:
                    {
                        effect = CreateSmallRoom(graph);
                        break;
                    }
                case ReverbEffectDefinitions.LargeRoom:
                    {
                        effect = CreateLargeRoom(graph);
                        break;
                    }
            }

            return effect;           
        }

        private static ReverbEffectDefinition CreateLargeRoom(AudioGraph graph)
        {
            ReverbEffectDefinition reverbEffectDefinition = new ReverbEffectDefinition(graph);

            reverbEffectDefinition.WetDryMix = 100;
            reverbEffectDefinition.ReflectionsDelay = 20;
            reverbEffectDefinition.ReverbDelay = 40;
            reverbEffectDefinition.RearDelay = 5;
            reverbEffectDefinition.PositionLeft = 6;
            reverbEffectDefinition.PositionRight = 6;
            reverbEffectDefinition.PositionMatrixLeft = 27;
            reverbEffectDefinition.PositionMatrixRight = 27;
            reverbEffectDefinition.EarlyDiffusion = 15;
            reverbEffectDefinition.LateDiffusion = 15;
            reverbEffectDefinition.LowEQGain = 8;
            reverbEffectDefinition.LowEQCutoff = 4;
            reverbEffectDefinition.HighEQGain = 8;
            reverbEffectDefinition.HighEQCutoff = 6;
            reverbEffectDefinition.RoomFilterFreq = 5000;
            reverbEffectDefinition.RoomFilterMain = -10;
            reverbEffectDefinition.RoomFilterHF = -6;
            reverbEffectDefinition.ReflectionsGain = -16;
            reverbEffectDefinition.ReverbGain = -10;
            reverbEffectDefinition.DecayTime = 1.50;
            reverbEffectDefinition.Density = 100;
            reverbEffectDefinition.RoomSize = 100;
            reverbEffectDefinition.DisableLateField = false;

            return reverbEffectDefinition;
        }

        private static ReverbEffectDefinition CreateSmallRoom(AudioGraph graph)
        {
            ReverbEffectDefinition reverbEffectDefinition = new ReverbEffectDefinition(graph);

            reverbEffectDefinition.WetDryMix = 100;
            reverbEffectDefinition.ReflectionsDelay = 5;
            reverbEffectDefinition.ReverbDelay = 10;
            reverbEffectDefinition.RearDelay = 5;
            reverbEffectDefinition.PositionLeft = 6;
            reverbEffectDefinition.PositionRight = 6;
            reverbEffectDefinition.PositionMatrixLeft = 27;
            reverbEffectDefinition.PositionMatrixRight = 27;
            reverbEffectDefinition.EarlyDiffusion = 15;
            reverbEffectDefinition.LateDiffusion = 15;
            reverbEffectDefinition.LowEQGain = 8;
            reverbEffectDefinition.LowEQCutoff = 4;
            reverbEffectDefinition.HighEQGain = 8;
            reverbEffectDefinition.HighEQCutoff = 6;
            reverbEffectDefinition.RoomFilterFreq = 5000;
            reverbEffectDefinition.RoomFilterMain = -10;
            reverbEffectDefinition.RoomFilterHF = -6;
            reverbEffectDefinition.ReflectionsGain = -4;
            reverbEffectDefinition.ReverbGain = 5;
            reverbEffectDefinition.DecayTime = 1.10;
            reverbEffectDefinition.Density = 100;
            reverbEffectDefinition.RoomSize = 100;
            reverbEffectDefinition.DisableLateField = false;

            return reverbEffectDefinition;
        }
    }
}
