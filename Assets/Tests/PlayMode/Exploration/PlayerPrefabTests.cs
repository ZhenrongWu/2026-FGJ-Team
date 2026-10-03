using FGJ.Exploration;
using NUnit.Framework;
using UnityEngine;

namespace FGJ.Tests.PlayMode.Exploration
{
    public class PlayerPrefabTests
    {
        private SideScrollPlayer _player;

        [SetUp]
        public void SetUp()
        {
            _player = TestPrefabs.Instantiate<SideScrollPlayer>(TestPrefabs.PlayerPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (_player != null)
                Object.Destroy(_player.gameObject);
        }

        [Test]
        public void PlayEnter_PlaysOnceThenHoldsLastFrame()
        {
            var animator = _player.GetComponentInChildren<SpriteFrameAnimator>();

            _player.PlayEnter();
            animator.Advance(10f);
            var heldFrame = animator.CurrentFrame;
            animator.Advance(0.25f);

            Assert.AreEqual(SideScrollPlayer.EnterClip, animator.CurrentClipName);
            Assert.IsTrue(animator.IsFinished);
            Assert.AreEqual(heldFrame, animator.CurrentFrame);
        }
    }
}
