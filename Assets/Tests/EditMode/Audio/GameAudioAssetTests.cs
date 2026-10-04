using System;
using FGJ.Audio;
using FGJ.Editor;
using NUnit.Framework;
using UnityEditor;

namespace FGJ.Tests.EditMode.Audio
{
    public class GameAudioAssetTests
    {
        private GameAudio _audio;

        [SetUp]
        public void SetUp()
        {
            _audio = AssetDatabase.LoadAssetAtPath<GameAudio>(AudioAssets.GameAudioPath);
            Assert.IsNotNull(_audio, $"找不到 {AudioAssets.GameAudioPath}");
        }

        [Test]
        public void GameAudio_HasMusicForExplorationAndEnding()
        {
            Assert.AreEqual("BGM_Exploration", _audio.ExplorationMusic?.name);
            Assert.AreEqual("BGM_Ending", _audio.EndingMusic?.name);
        }

        [Test]
        public void GameAudio_HasFootstepLoop()
        {
            Assert.AreEqual("SFX_Footstep", _audio.Footstep?.name);
        }

        [TestCase(SoundEffect.ButtonPress, "SFX_ButtonPress")]
        [TestCase(SoundEffect.Hover, "SFX_Hover")]
        [TestCase(SoundEffect.DoorOpen, "SFX_DoorOpen")]
        [TestCase(SoundEffect.DiceShake, "SFX_DiceShake")]
        [TestCase(SoundEffect.OxygenGain, "SFX_OxygenGain")]
        [TestCase(SoundEffect.OxygenLoss, "SFX_OxygenLoss")]
        public void GameAudio_MapsEachEffectToItsClip(SoundEffect effect, string clipName)
        {
            Assert.AreEqual(clipName, _audio.ClipFor(effect)?.name);
        }

        [Test]
        public void EveryEffect_HasAClip()
        {
            foreach (SoundEffect effect in Enum.GetValues(typeof(SoundEffect)))
                Assert.IsNotNull(_audio.ClipFor(effect), effect.ToString());
        }

        [Test]
        public void EachLevelConfig_UsesItsOwnMusic()
        {
            for (var level = 1; level <= ExplorationSceneMenu.LevelCount; level++)
            {
                var config = LiarDiceSceneMenu.LevelConfig(level);
                Assert.IsNotNull(config, $"第 {level} 關設定不存在");
                Assert.AreEqual($"BGM_Level{level}", config.Music?.name);
            }
        }
    }
}
