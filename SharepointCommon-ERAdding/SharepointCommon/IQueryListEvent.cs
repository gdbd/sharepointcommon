using System;

namespace SharepointCommon
{
    public interface IQueryListEvent
    {
        event Action<Item> Add;

        // todo: add other listitem events
    }
}