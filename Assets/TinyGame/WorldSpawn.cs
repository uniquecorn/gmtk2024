using Castle.Core;

namespace TinyGame
{
    public abstract class WorldSpawn : CastleObject
    {

    }
    public abstract class WorldSpawn<T> : WorldSpawn where T : WorldObject
    {
        public T worldObject;
    }
}