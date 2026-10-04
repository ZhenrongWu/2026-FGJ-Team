using FGJ.Audio;
using NUnit.Framework;
using UnityEngine;

namespace FGJ.Tests.PlayMode.Audio
{
    public class GameAudioTests
    {
        private GameAudio _audio;
        private AudioClip _exploration;
        private AudioClip _footstep;

        [SetUp]
        public void SetUp()
        {
            DestroyPlayers();
            _audio = ScriptableObject.CreateInstance<GameAudio>();
            _exploration = RecordingGameAudio.CreateClip("Exploration");
            _footstep = RecordingGameAudio.CreateClip("Footstep");
            _audio.SetMusic(_exploration, null);
            _audio.SetFootstep(_footstep);
        }

        [TearDown]
        public void TearDown()
        {
            DestroyPlayers();
            Object.Destroy(_audio);
            Object.Destroy(_exploration);
            Object.Destroy(_footstep);
        }

        private static void DestroyPlayers()
        {
            foreach (var player in Players())
                Object.DestroyImmediate(player.gameObject);
        }

        private static GameAudioPlayer[] Players() =>
            Object.FindObjectsByType<GameAudioPlayer>(FindObjectsSortMode.None);

        [Test]
        public void PlayExplorationMusic_CreatesOnePersistentPlayer()
        {
            _audio.PlayExplorationMusic();
            _audio.PlayExplorationMusic();

            var players = Players();
            Assert.AreEqual(1, players.Length);
            Assert.AreSame(_exploration, players[0].CurrentMusic);
            Assert.AreEqual("DontDestroyOnLoad", players[0].gameObject.scene.name);
        }

        [Test]
        public void PlayEndingMusic_WithoutClip_DoesNotCreatePlayer()
        {
            _audio.PlayEndingMusic();

            Assert.IsEmpty(Players());
        }

        [Test]
        public void SetFootsteps_StoppingBeforeAnyPlayback_DoesNotCreatePlayer()
        {
            _audio.SetFootsteps(false);

            Assert.IsEmpty(Players());
        }

        [Test]
        public void SetFootsteps_Walking_LoopsFootstepClip()
        {
            _audio.SetFootsteps(true);

            Assert.AreSame(_footstep, Players()[0].CurrentFootsteps);
        }

        [Test]
        public void SetEffect_AssignsClipForThatEffectOnly()
        {
            var door = RecordingGameAudio.CreateClip("Door");

            _audio.SetEffect(SoundEffect.DoorOpen, door);

            Assert.AreSame(door, _audio.ClipFor(SoundEffect.DoorOpen));
            Assert.IsNull(_audio.ClipFor(SoundEffect.DiceShake));
            Object.Destroy(door);
        }
    }
}
