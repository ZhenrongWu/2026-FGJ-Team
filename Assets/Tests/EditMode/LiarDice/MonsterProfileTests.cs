using FGJ.LiarDice;
using NUnit.Framework;
using UnityEngine;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class MonsterProfileTests
    {
        private MonsterProfile _monster;

        [SetUp]
        public void SetUp() => _monster = ScriptableObject.CreateInstance<MonsterProfile>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_monster);

        [Test]
        public void Defaults_DescribeSwampWatcher()
        {
            Assert.AreEqual("沼澤看守者", _monster.DisplayName);
            Assert.AreEqual("「3 個 4 點。」", _monster.BidLine(new Bid(3, 4)));
        }

        [Test]
        public void Configure_ChangesNameAndLines()
        {
            _monster.Configure("深海鮟鱇", "「歡迎。」", "「抓到了。」", "我喊 {0}");

            Assert.AreEqual("深海鮟鱇", _monster.DisplayName);
            Assert.AreEqual("「歡迎。」", _monster.Greeting);
            Assert.AreEqual("「抓到了。」", _monster.ChallengeLine);
            Assert.AreEqual("我喊 2 個 6 點", _monster.BidLine(new Bid(2, 6)));
        }

        [Test]
        public void Sprite_DefaultsToNoneAndCanBeAssigned()
        {
            Assert.IsNull(_monster.Sprite);

            var texture = new Texture2D(4, 4);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.zero);
            try
            {
                _monster.SetSprite(sprite);
                Assert.AreSame(sprite, _monster.Sprite);
            }
            finally
            {
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void ToAIProfile_UsesPersonalityDefaults()
        {
            var profile = _monster.ToAIProfile();

            Assert.AreEqual(0.35f, profile.ChallengeThreshold, 1e-5f);
            Assert.AreEqual(0.5f, profile.ConfidentBidThreshold, 1e-5f);
            Assert.AreEqual(0.15f, profile.BluffChance, 1e-5f);
            Assert.AreEqual(2, profile.MaxRaiseStep);
        }
    }
}
