using System;
using UnityEngine;

namespace FGJ.LiarDice.UI
{
    [Serializable]
    public sealed class LiarDicePalette
    {
        public Color dieFace = new Color32(232, 220, 192, 255);
        public Color dieHighlight = new Color32(242, 191, 76, 255);
        public Color dieHidden = new Color32(46, 58, 50, 255);
        public Color pips = new Color32(24, 20, 16, 255);
        public Color hiddenPips = new Color32(150, 170, 156, 255);
        public Color wildActive = new Color32(111, 211, 154, 255);
        public Color wildCancelled = new Color32(217, 83, 79, 255);

        public (Color face, Color pips) DieColors(bool hidden, bool highlighted)
        {
            if (hidden)
                return (dieHidden, hiddenPips);
            return (highlighted ? dieHighlight : dieFace, pips);
        }

        public Color WildStatusColor(bool wildIsActive) => wildIsActive ? wildActive : wildCancelled;
    }
}
