using UnityEngine;

namespace TinyGame
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteObject : MonoBehaviour
    {
        [Autohook]
        public SpriteRenderer sr;
    }
}