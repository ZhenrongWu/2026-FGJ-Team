using FGJ.Exploration;
using FGJ.LiarDice;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class LiarDiceConfigTests
    {
        private const string BuildingPathFormat = "Assets/Data/Exploration/Building_{0:00}.asset";

        private LiarDiceConfig _config;

        [SetUp]
        public void SetUp() => _config = ScriptableObject.CreateInstance<LiarDiceConfig>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        private static BuildingDefinition LoadBuilding(int level)
        {
            var path = string.Format(BuildingPathFormat, level);
            var building = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path);
            Assert.IsNotNull(building, $"找不到 {path}");
            return building;
        }

        [Test]
        public void Defaults_MatchLevelOneSpec()
        {
            var settings = _config.ToMatchSettings();

            Assert.AreEqual(5, settings.PlayerDiceCount);
            Assert.AreEqual(5, settings.MonsterDiceCount);
            Assert.AreEqual(5, settings.PlayerOxygen);
            Assert.AreEqual(5, settings.PlayerMaxOxygen);
            Assert.AreEqual(2, settings.MonsterOxygen);
            Assert.AreEqual(0, settings.ItemCount);
            Assert.AreEqual(Side.Monster, settings.FirstTurn);
            Assert.IsTrue(settings.Rules.OnesAreWild);
            Assert.IsFalse(_config.EndsGame);
        }

        [Test]
        public void ToMatchSettings_CarriedOxygen_StartsLowerButKeepsMaximum()
        {
            var settings = _config.ToMatchSettings(3);

            Assert.AreEqual(3, settings.PlayerOxygen);
            Assert.AreEqual(5, settings.PlayerMaxOxygen);
        }

        [Test]
        public void ToMatchSettings_OxygenAboveMaximum_IsClamped()
        {
            Assert.AreEqual(5, _config.ToMatchSettings(9).PlayerOxygen);
        }

        [TestCase(1, ExpectedResult = 3)]
        [TestCase(3, ExpectedResult = 5)]
        [TestCase(5, ExpectedResult = 5)]
        public int OxygenAfterVictory_AddsRewardUpToMaximum(int remaining)
        {
            return _config.OxygenAfterVictory(remaining);
        }

        [TestCase(1, 5, 2, 0, false)]
        [TestCase(2, 7, 4, 1, false)]
        [TestCase(3, 10, 8, 2, false)]
        [TestCase(4, 15, 16, 4, true)]
        public void LevelAsset_MatchesDesignTable(int level, int dice, int monsterOxygen, int items, bool endsGame)
        {
            var config = LoadBuilding(level).GameplayConfig;

            Assert.IsNotNull(config, $"第 {level} 關沒有設定 GameplayConfig");
            Assert.AreEqual(dice, config.PlayerDiceCount);
            Assert.AreEqual(dice, config.MonsterDiceCount);
            Assert.AreEqual(5, config.PlayerOxygen);
            Assert.AreEqual(monsterOxygen, config.MonsterOxygen);
            Assert.AreEqual(2, config.WinOxygenReward);
            Assert.AreEqual(items, config.ItemCount);
            Assert.AreEqual(Side.Monster, config.FirstTurn);
            Assert.IsTrue(config.OnesAreWild);
            Assert.AreEqual(endsGame, config.EndsGame);
        }

        [TestCase(1, "BlobfishScumbag")]
        [TestCase(2, "LophiiformesBartender")]
        [TestCase(3, "GoblinSharkGang")]
        [TestCase(4, "GiantSquidMayor")]
        public void LevelAsset_UsesAssignedMonsterSprite(int level, string spriteName)
        {
            var monster = LoadBuilding(level).Monster;

            Assert.IsNotNull(monster, $"第 {level} 關沒有設定怪物");
            Assert.IsNotNull(monster.Sprite, $"第 {level} 關怪物沒有圖");
            Assert.AreEqual(spriteName, monster.Sprite.name);
        }

        private static TextureImporterSettings SpriteSettings(Sprite sprite)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite));
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            return settings;
        }

        [TestCase(1, "BlobfishScumbag_Dead")]
        [TestCase(2, "LophiiformesBartender_Dead")]
        [TestCase(3, "GoblinSharkGang_Dead")]
        [TestCase(4, "GiantSquidMayor_Dead")]
        public void LevelAsset_HasDeadSpriteAtSameScaleAsAliveSprite(int level, string spriteName)
        {
            var monster = LoadBuilding(level).Monster;

            Assert.IsNotNull(monster.DeadSprite, $"第 {level} 關怪物沒有死亡圖");
            Assert.AreEqual(spriteName, monster.DeadSprite.name);
            var alive = SpriteSettings(monster.Sprite);
            var dead = SpriteSettings(monster.DeadSprite);
            Assert.AreEqual(alive.spritePixelsPerUnit, dead.spritePixelsPerUnit);
            Assert.AreEqual(alive.spriteAlignment, dead.spriteAlignment);
        }
    }
}
