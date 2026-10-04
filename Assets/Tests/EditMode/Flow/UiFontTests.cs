using System.Linq;
using FGJ.Editor;
using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace FGJ.Tests.EditMode.Flow
{
    public class UiFontTests
    {
        private Font _font;

        [SetUp]
        public void SetUp()
        {
            _font = AssetDatabase.LoadAssetAtPath<Font>(UiFactory.FontPath);
            Assert.IsNotNull(_font, $"找不到 {UiFactory.FontPath}");
        }

        [Test]
        public void UiFactory_UsesBundledChineseFont()
        {
            Assert.AreSame(_font, new UiFactory().Font);
        }

        [TestCase(DefaultPrefabs.GameplayHudPath)]
        [TestCase(DefaultPrefabs.HudPath)]
        public void Prefab_EveryTextUsesBundledFont(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            foreach (var text in prefab.GetComponentsInChildren<Text>(true))
                Assert.AreSame(_font, text.font, text.name);
        }

        [Test]
        public void EndingScene_EveryTextUsesBundledFont()
        {
            var scene = EditorSceneManager.OpenScene(EndingSceneMenu.ScenePath, OpenSceneMode.Additive);
            try
            {
                var texts = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Text>(true));
                foreach (var text in texts)
                    Assert.AreSame(_font, text.font, text.name);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void BundledFont_HasGlyphsForGameplayText()
        {
            var text = new LiarDiceText();
            var samples = text.ItemTooltip(ItemType.StealOxygen) + text.ItemTooltip(ItemType.PeekLens) +
                          text.CurrentBid(new Bid(3, 4), Side.Monster) + LiarDiceText.EndingLabel;

            foreach (var character in samples.Distinct())
                Assert.IsTrue(_font.HasCharacter(character), character.ToString());
        }
    }
}
