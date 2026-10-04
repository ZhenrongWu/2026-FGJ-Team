using System.Collections.Generic;
using FGJ.LiarDice;

namespace FGJ.Tests.EditMode.LiarDice
{
    internal sealed class FixedItemDealer : IItemDealer
    {
        private readonly Queue<ItemType> _items;

        public FixedItemDealer(params ItemType[] items)
        {
            _items = new Queue<ItemType>(items);
        }

        public ItemType Draw() => _items.Dequeue();
    }
}
