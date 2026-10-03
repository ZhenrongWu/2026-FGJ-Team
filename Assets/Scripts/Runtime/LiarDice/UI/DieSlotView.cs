using UnityEngine;
using UnityEngine.UI;

namespace FGJ.LiarDice.UI
{
    public sealed class DieSlotView : MonoBehaviour
    {
        public const string HiddenPips = "?";

        [SerializeField] private Image face;
        [SerializeField] private Text pips;

        public string PipsText => pips.text;
        public Color FaceColor => face.color;

        public void Configure(Image faceImage, Text pipsText)
        {
            face = faceImage;
            pips = pipsText;
        }

        public void Show(int value, bool hidden, bool highlighted, LiarDicePalette palette)
        {
            var (faceColor, pipsColor) = palette.DieColors(hidden, highlighted);
            face.color = faceColor;
            pips.color = pipsColor;
            pips.text = hidden ? HiddenPips : value.ToString();
        }
    }
}
