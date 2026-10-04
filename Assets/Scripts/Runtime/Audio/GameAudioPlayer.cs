using UnityEngine;

namespace FGJ.Audio
{
    public sealed class GameAudioPlayer : MonoBehaviour
    {
        private AudioSource _music;
        private AudioSource _effects;
        private AudioSource _footsteps;

        public AudioClip CurrentMusic => _music != null ? _music.clip : null;
        public AudioClip CurrentFootsteps => IsWalking ? _footsteps.clip : null;
        public bool IsWalking { get; private set; }

        private AudioSource Music => _music != null ? _music : _music = CreateSource(true);
        private AudioSource Effects => _effects != null ? _effects : _effects = CreateSource(false);
        private AudioSource Footsteps => _footsteps != null ? _footsteps : _footsteps = CreateSource(true);

        public void PlayMusic(AudioClip clip, float volume)
        {
            Music.volume = volume;
            if (Music.clip == clip)
                return;
            Music.clip = clip;
            Music.Play();
        }

        public void PlayEffect(AudioClip clip, float volume)
        {
            Effects.PlayOneShot(clip, volume);
        }

        public void SetFootsteps(AudioClip clip, bool walking, float volume)
        {
            if (!walking)
            {
                IsWalking = false;
                if (_footsteps != null)
                    _footsteps.Stop();
                return;
            }

            Footsteps.volume = volume;
            if (IsWalking && Footsteps.clip == clip)
                return;
            IsWalking = true;
            Footsteps.clip = clip;
            Footsteps.Play();
        }

        private AudioSource CreateSource(bool loop)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            return source;
        }
    }
}
