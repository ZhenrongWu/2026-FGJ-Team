using System;
using FGJ.Editor;
using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class GameplayHudAssetTests
    {
        private LiarDiceHud _hud;
        private Transform _log;

        [SetUp]
        public void SetUp()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultPrefabs.GameplayHudPath);
            Assert.IsNotNull(prefab, $"找不到 {DefaultPrefabs.GameplayHudPath}");
            _hud = prefab.GetComponent<LiarDiceHud>();
            _log = prefab.transform.Find(GameplayHudBuilder.MatchLogName);
            Assert.IsNotNull(_log);
        }

        [Test]
        public void MatchLog_HasSkullDecorationOnTop()
        {
            var decoration = _log.Find(GameplayHudBuilder.LogDecorationName);

            Assert.IsNotNull(decoration);
            Assert.AreEqual(_log.childCount - 1, decoration.GetSiblingIndex());
            var image = decoration.GetComponent<Image>();
            Assert.AreEqual("MatchLog_Skull", image.sprite?.name);
            Assert.IsFalse(image.raycastTarget);
        }

        [Test]
        public void SkullDecorationSprite_IsImportedAsSprite()
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(GameplayHudBuilder.LogDecorationSpritePath);

            Assert.IsNotNull(sprite, $"找不到 {GameplayHudBuilder.LogDecorationSpritePath}");
        }

        [TestCase(HudArt.OxygenTank)]
        [TestCase(HudArt.OxygenBubble)]
        [TestCase(HudArt.ItemBarFrame)]
        [TestCase(HudArt.ControlPanelFrame)]
        public void HudSprite_IsImportedAsSprite(string name)
        {
            Assert.IsNotNull(HudArt.Load(name), $"找不到 {HudArt.SpritePath(name)}");
        }

        [Test]
        public void OxygenGauges_UseTankWithBubbleSegments()
        {
            foreach (var gauge in new[] { _hud.PlayerOxygen, _hud.MonsterOxygen })
            {
                Assert.AreEqual(HudArt.OxygenTank, gauge.GetComponent<Image>().sprite?.name, gauge.name);
                var segment = gauge.transform.Find("SegmentTemplate").GetComponent<Image>();
                Assert.AreEqual(HudArt.OxygenBubble, segment.sprite?.name, gauge.name);
                Assert.IsTrue(segment.preserveAspect, gauge.name);
            }
        }

        [Test]
        public void ItemBars_UseFrameAndIconSlots()
        {
            foreach (var bar in new[] { _hud.PlayerItems, _hud.MonsterItems })
            {
                Assert.AreEqual(HudArt.ItemBarFrame, bar.GetComponent<Image>().sprite?.name, bar.name);
                var template = bar.transform.Find("ItemSlotTemplate");
                Assert.IsNotNull(template.Find(ItemBarView.IconName), bar.name);
            }
        }

        [Test]
        public void ControlPanel_ShowsFrameAboveTheControls()
        {
            var panel = _hud.transform.Find(GameplayHudBuilder.ControlPanelName);
            var frame = (RectTransform)panel.Find(GameplayHudBuilder.ControlFrameName);

            Assert.AreEqual(HudArt.ControlPanelFrame, frame.GetComponent<Image>().sprite?.name);
            Assert.IsFalse(frame.GetComponent<Image>().raycastTarget);
            var frameBottom = Corners(frame)[0].y;
            foreach (var control in new[] { _hud.Parts.believeButton, _hud.Parts.bluffButton, _hud.Parts.continueButton })
                Assert.LessOrEqual(Corners((RectTransform)control.transform)[1].y, frameBottom, control.name);
        }

        private static Vector3[] Corners(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return corners;
        }

        [Test]
        public void Hud_HasItemTooltipBelowPlayerItems()
        {
            Assert.IsNotNull(_hud.Parts.itemTooltip);
            Assert.AreEqual(GameplayHudBuilder.ItemTooltipName, _hud.Parts.itemTooltip.name);
        }

        [Test]
        public void Hud_ItemIconsCoverEveryItemWithMatchingArt()
        {
            Assert.IsNotNull(_hud.ItemIcons);
            foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
                Assert.AreEqual(HudArt.ItemIconName(item), _hud.ItemIcons.IconFor(item)?.name, item.ToString());
        }
    }
}
