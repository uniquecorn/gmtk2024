using Castle.Core;

namespace TinyGame
{
    public interface IGridObject
    {
        CastleGrid CurrentGrid { get; }
        int WalkableIndex { get; }
    }
}