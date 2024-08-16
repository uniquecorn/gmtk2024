using Castle.Core;
namespace TinyGame
{
    public abstract class GridObject : CastleObject, IGridObject
    {
        public CastleGrid CurrentGrid { get; }
        public int WalkableIndex { get; }
        public SpriteObject[] sprites;
    }
}