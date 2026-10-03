using FGJ.LiarDice.UI;
using NUnit.Framework;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class LiarDicePaletteTests
    {
        private readonly LiarDicePalette _palette = new LiarDicePalette();

        [Test]
        public void DieColors_Hidden_IgnoresHighlight()
        {
            Assert.AreEqual((_palette.dieHidden, _palette.hiddenPips), _palette.DieColors(true, true));
        }

        [Test]
        public void DieColors_Revealed_HighlightsMatchingDice()
        {
            Assert.AreEqual((_palette.dieHighlight, _palette.pips), _palette.DieColors(false, true));
            Assert.AreEqual((_palette.dieFace, _palette.pips), _palette.DieColors(false, false));
        }

        [Test]
        public void WildStatusColor_ReflectsWildState()
        {
            Assert.AreEqual(_palette.wildActive, _palette.WildStatusColor(true));
            Assert.AreEqual(_palette.wildCancelled, _palette.WildStatusColor(false));
        }
    }
}
