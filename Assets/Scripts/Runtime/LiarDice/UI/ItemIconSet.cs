using System;
using System.Collections.Generic;
using UnityEngine;

namespace FGJ.LiarDice.UI
{
    [CreateAssetMenu(fileName = "ItemIcons", menuName = "FGJ/Liar Dice/Item Icons")]
    public sealed class ItemIconSet : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public ItemType item;
            public Sprite icon;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        public Sprite IconFor(ItemType item)
        {
            foreach (var entry in entries)
                if (entry.item == item)
                    return entry.icon;
            return null;
        }

        public void SetIcon(ItemType item, Sprite icon)
        {
            entries.RemoveAll(entry => entry.item == item);
            entries.Add(new Entry { item = item, icon = icon });
        }
    }
}
