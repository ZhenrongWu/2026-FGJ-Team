using FGJ.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class GameplayHudAssetTests
    {
        private Transform _log;

        [SetUp]
        public void SetUp()
        {
            var hud = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultPrefabs.GameplayHudPath);
            Assert.IsNotNull(hud, $"找不到 {DefaultPrefabs.GameplayHudPath}");
            _log = hud.transform.Find(GameplayHudBuilder.MatchLogName);
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
    }
}
