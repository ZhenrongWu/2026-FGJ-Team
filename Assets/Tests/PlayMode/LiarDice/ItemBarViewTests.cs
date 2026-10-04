using System.Collections.Generic;
using FGJ.LiarDice.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace FGJ.Tests.PlayMode.LiarDice
{
    public class ItemBarViewTests
    {
        private readonly List<Object> _created = new List<Object>();
        private ItemBarView _bar;
        private Sprite _icon;

        [SetUp]
        public void SetUp()
        {
            var root = Track(new GameObject("ItemBar", typeof(RectTransform)));
            var slots = new GameObject("Slots", typeof(RectTransform));
            slots.transform.SetParent(root.transform, false);

            var template = new GameObject("ItemSlotTemplate", typeof(RectTransform), typeof(Image), typeof(Button));
            template.transform.SetParent(root.transform, false);
            var icon = new GameObject(ItemBarView.IconName, typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(template.transform, false);
            var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(template.transform, false);
            template.SetActive(false);

            _bar = root.AddComponent<ItemBarView>();
            _bar.Configure((RectTransform)slots.transform, template.GetComponent<Button>());

            var texture = Track(new Texture2D(4, 4));
            _icon = Track(Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.zero));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var created in _created)
                Object.Destroy(created);
            _created.Clear();
        }

        private T Track<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }

        private Transform Slot(int index) => _bar.transform.Find($"Slots/ItemSlot_{index}");

        [Test]
        public void Show_SlotWithIcon_ShowsIconAndHidesLabel()
        {
            _bar.Show(new[] { new ItemSlot("重搖", _icon) }, true);

            var icon = Slot(0).Find(ItemBarView.IconName).GetComponent<Image>();
            Assert.AreSame(_icon, icon.sprite);
            Assert.IsTrue(icon.enabled);
            Assert.AreEqual(string.Empty, Slot(0).GetComponentInChildren<Text>().text);
            CollectionAssert.AreEqual(new[] { "重搖" }, _bar.Labels);
        }

        [Test]
        public void Show_SlotWithoutIcon_ShowsLabelOnly()
        {
            _bar.Show(new[] { new ItemSlot(LiarDiceText.HiddenItemLabel, null) }, false);

            Assert.IsFalse(Slot(0).Find(ItemBarView.IconName).GetComponent<Image>().enabled);
            Assert.AreEqual(LiarDiceText.HiddenItemLabel, Slot(0).GetComponentInChildren<Text>().text);
            CollectionAssert.AreEqual(new Sprite[] { null }, _bar.Icons);
        }

        [Test]
        public void Hover_VisibleSlot_RaisesSlotHovered()
        {
            var hovered = -1;
            _bar.SlotHovered += index => hovered = index;
            _bar.Show(new[] { new ItemSlot("重搖", _icon), new ItemSlot("封口膠帶", _icon) }, true);

            _bar.Hover(1);

            Assert.AreEqual(1, hovered);
        }

        [Test]
        public void Hover_HiddenSlot_IsIgnored()
        {
            var hovered = false;
            _bar.SlotHovered += _ => hovered = true;
            _bar.Show(new[] { new ItemSlot("重搖", _icon) }, true);

            _bar.Hover(2);

            Assert.IsFalse(hovered);
        }
    }
}
