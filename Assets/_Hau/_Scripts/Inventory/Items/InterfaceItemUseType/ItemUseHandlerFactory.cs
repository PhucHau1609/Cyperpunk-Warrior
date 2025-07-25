using System.Collections.Generic;

public static class ItemUseHandlerFactory
{
    private static Dictionary<ItemUseType, IItemUseHandler> handlers = new Dictionary<ItemUseType, IItemUseHandler>
    {
        { ItemUseType.Heal, new HealHandler() },
        { ItemUseType.Info, new InfoHandler() },
        // có thể thêm các handler mới ở đây
    };

    public static IItemUseHandler GetHandler(ItemUseType useType)
    {
        if (handlers.TryGetValue(useType, out var handler))
            return handler;

        return null;
    }
}
