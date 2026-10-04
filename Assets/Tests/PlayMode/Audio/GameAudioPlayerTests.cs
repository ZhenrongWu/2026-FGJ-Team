using FGJ.Audio;
using NUnit.Framework;
using UnityEngine;

namespace FGJ.Tests.PlayMode.Audio
{
    public class GameAudioPlayerTests
    {
        private GameAudioPlayer _player;
        private AudioClip _first;
        private AudioClip _second;

        [SetUp]
        public void SetUp()
        {
            _player = new GameObject("AudioPlayer").AddComponent<GameAudioPlayer>();
            _first = RecordingGameAudio.CreateClip("First");
            _second = RecordingGameAudio.CreateClip("Second");
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_player.gameObject);
            Object.Destroy(_first);
            Object.Destroy(_second);
        }

        [Test]
        public void PlayMusic_UsesSingleLoopingSource()
        {
            _player.PlayMusic(_first, 0.5f);
            _player.PlayMusic(_first, 0.5f);

            var sources = _player.GetComponents<AudioSource>();
            Assert.AreEqual(1, sources.Length);
            Assert.IsTrue(sources[0].loop);
            Assert.AreSame(_first, _player.CurrentMusic);
        }

        [Test]
        public void PlayMusic_DifferentClip_SwitchesTrack()
        {
            _player.PlayMusic(_first, 0.5f);

            _player.PlayMusic(_second, 0.5f);

            Assert.AreSame(_second, _player.CurrentMusic);
        }

        [Test]
        public void SetFootsteps_WalkingThenStopping_StartsAndStopsLoop()
        {
            _player.SetFootsteps(_first, true, 1f);
            Assert.IsTrue(_player.IsWalking);
            Assert.AreSame(_first, _player.CurrentFootsteps);

            _player.SetFootsteps(_first, false, 1f);
            Assert.IsFalse(_player.IsWalking);
            Assert.IsNull(_player.CurrentFootsteps);
        }

        [Test]
        public void SetFootsteps_StoppingBeforeWalking_CreatesNoSource()
        {
            _player.SetFootsteps(_first, false, 1f);

            Assert.IsFalse(_player.IsWalking);
            Assert.IsEmpty(_player.GetComponents<AudioSource>());
        }
    }
}
