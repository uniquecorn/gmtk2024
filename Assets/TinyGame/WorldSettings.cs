using Sirenix.Utilities;
using UnityEngine;

namespace TinyGame
{
    [GlobalConfig("Assets/Resources"),CreateAssetMenu]
    public class WorldSettings : GlobalConfig<WorldSettings>
    {
        public Material terrainMaterial;
        public Color waterColor, landColor;
        public NoiseSettings noiseSettings;
        public TreeSpawn treePrefab;
        public PersonSpawn personPrefab;
        public Sprite[] trees;
    }
}