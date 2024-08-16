using Sirenix.Utilities;
using UnityEngine;

namespace TinyGame
{
    [GlobalConfig("Assets/Resources"),CreateAssetMenu]
    public class WorldSettings : GlobalConfig<WorldSettings>
    {
        public Sprite terrain;
        public NoiseSettings noiseSettings;
    }
}