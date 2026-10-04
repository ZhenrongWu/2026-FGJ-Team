using System.Collections.Generic;
using FGJ.Audio;
using UnityEngine;

namespace FGJ.Tests.PlayMode
{
    internal sealed class RecordingGameAudio : GameAudio
    {
        public readonly List<AudioClip> Music = new List<AudioClip>();
        public readonly List<SoundEffect> Effects = new List<SoundEffect>();
        public readonly List<bool> FootstepChanges = new List<bool>();

        public static RecordingGameAudio Create()
        {
            var audio = CreateInstance<RecordingGameAudio>();
            audio.SetMusic(CreateClip("Exploration"), CreateClip("Ending"));
            return audio;
        }

        public static AudioClip CreateClip(string clipName)
        {
            return AudioClip.Create(clipName, 441, 1, 44100, false);
        }

        public override void PlayMusic(AudioClip clip) => Music.Add(clip);

        public override void Play(SoundEffect effect) => Effects.Add(effect);

        public override void SetFootsteps(bool walking) => FootstepChanges.Add(walking);
    }
}
