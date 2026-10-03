using FGJ.LiarDice;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class LiarDiceConfigTests
    {
        private const string SwampWatcherPath = "Assets/Data/LiarDice/LiarDiceConfig_SwampWatcher.asset";

        [Test]
        public void Defaults_MatchLevelOneSpec()
        {
            var config = ScriptableObject.CreateInstance<LiarDiceConfig>();
            try
            {
                var settings = config.ToMatchSettings();
                Assert.AreEqual(5, settings.PlayerDiceCount);
                Assert.AreEqual(5, settings.MonsterDiceCount);
                Assert.AreEqual(2, settings.PlayerOxygen);
                Assert.AreEqual(2, settings.MonsterOxygen);
                Assert.AreEqual(Side.Monster, settings.FirstTurn);
                Assert.IsTrue(settings.Rules.OnesAreWild);
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void SwampWatcherAsset_ExistsWithTwoOxygen()
        {
            var config = AssetDatabase.LoadAssetAtPath<LiarDiceConfig>(SwampWatcherPath);

            Assert.IsNotNull(config, $"找不到 {SwampWatcherPath}");
            Assert.AreEqual(2, config.PlayerOxygen);
            Assert.AreEqual(2, config.MonsterOxygen);
            Assert.AreEqual(5, config.PlayerDiceCount);
        }
    }
}
