using Sirenix.Utilities;
using UnityEngine;

namespace TinyGame
{
    [CreateAssetMenu]
    public class WorldSettings : ScriptableObject
    {
        public Material terrainMaterial;
        public Color waterColor, landColor;
        public NoiseSettings noiseSettings;
        public TreeSpawn treePrefab;
        public PersonSpawn personPrefab;
        public Sprite[] trees;
    }
}