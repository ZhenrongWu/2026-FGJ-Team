using System.Collections.Generic;
using FGJ.LiarDice;

namespace FGJ.Tests.PlayMode.LiarDice
{
    internal sealed class QueuedItemDealer : IItemDealer
    {
        private readonly Queue<ItemType> _items;

        public QueuedItemDealer(IEnumerable<ItemType> items)
        {
            _items = new Queue<ItemType>(items);
        }

        public ItemType Draw() => _items.Dequeue();
    }
}
