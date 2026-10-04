using UnityEngine;

namespace FGJ.LiarDice.UI
{
    public readonly struct ItemSlot
    {
        public readonly string Label;
        public readonly Sprite Icon;

        public ItemSlot(string label, Sprite icon)
        {
            Label = label;
            Icon = icon;
        }
    }
}
