using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using NUnit.Framework;
using UnityEngine;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class ItemIconSetTests
    {
        private ItemIconSet _icons;
        private Texture2D _texture;

        [SetUp]
        public void SetUp()
        {
            _icons = ScriptableObject.CreateInstance<ItemIconSet>();
            _texture = new Texture2D(4, 4);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_icons);
            Object.DestroyImmediate(_texture);
        }

        private Sprite CreateSprite() => Sprite.Create(_texture, new Rect(0, 0, 4, 4), Vector2.zero);

        [Test]
        public void IconFor_AssignedItem_ReturnsItsIcon()
        {
            var lens = CreateSprite();
            _icons.SetIcon(ItemType.PeekLens, lens);

            Assert.AreSame(lens, _icons.IconFor(ItemType.PeekLens));
        }

        [Test]
        public void IconFor_UnassignedItem_ReturnsNull()
        {
            _icons.SetIcon(ItemType.PeekLens, CreateSprite());

            Assert.IsNull(_icons.IconFor(ItemType.Reroll));
        }

        [Test]
        public void SetIcon_SameItemTwice_KeepsLatestIcon()
        {
            var latest = CreateSprite();
            _icons.SetIcon(ItemType.SealTape, CreateSprite());

            _icons.SetIcon(ItemType.SealTape, latest);

            Assert.AreSame(latest, _icons.IconFor(ItemType.SealTape));
        }
    }
}
