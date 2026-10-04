using System;
using UnityEngine;

namespace FGJ.Audio
{
    public enum SoundEffect
    {
        ButtonPress,
        Hover,
        DoorOpen,
        DiceShake,
        OxygenGain,
        OxygenLoss
    }

    [CreateAssetMenu(fileName = "GameAudio", menuName = "FGJ/Audio/Game Audio")]
    public class GameAudio : ScriptableObject
    {
        [Header("音樂")]
        [Tooltip("主選單與建築外探索共用，切換場景時不會重播")]
        [SerializeField] private AudioClip explorationMusic;
        [SerializeField] private AudioClip endingMusic;
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.6f;

        [Header("音效")]
        [SerializeField] private AudioClip buttonPress;
        [SerializeField] private AudioClip hover;
        [SerializeField] private AudioClip footstep;
        [SerializeField] private AudioClip doorOpen;
        [SerializeField] private AudioClip diceShake;
        [SerializeField] private AudioClip oxygenGain;
        [SerializeField] private AudioClip oxygenLoss;
        [Range(0f, 1f)] [SerializeField] private float effectVolume = 1f;

        [NonSerialized] private GameAudioPlayer _player;

        public AudioClip ExplorationMusic => explorationMusic;
        public AudioClip EndingMusic => endingMusic;
        public AudioClip Footstep => footstep;

        private GameAudioPlayer Player => _player != null ? _player : _player = CreatePlayer();

        public void PlayExplorationMusic() => PlayMusic(explorationMusic);

        public void PlayEndingMusic() => PlayMusic(endingMusic);

        public virtual void PlayMusic(AudioClip clip)
        {
            if (clip != null)
                Player.PlayMusic(clip, musicVolume);
        }

        public virtual void Play(SoundEffect effect)
        {
            var clip = ClipFor(effect);
            if (clip != null)
                Player.PlayEffect(clip, effectVolume);
        }

        public virtual void SetFootsteps(bool walking)
        {
            if (footstep == null || (!walking && _player == null))
                return;
            Player.SetFootsteps(footstep, walking, effectVolume);
        }

        public AudioClip ClipFor(SoundEffect effect)
        {
            switch (effect)
            {
                case SoundEffect.ButtonPress: return buttonPress;
                case SoundEffect.Hover: return hover;
                case SoundEffect.DoorOpen: return doorOpen;
                case SoundEffect.DiceShake: return diceShake;
                case SoundEffect.OxygenGain: return oxygenGain;
                case SoundEffect.OxygenLoss: return oxygenLoss;
                default: return null;
            }
        }

        public void SetMusic(AudioClip exploration, AudioClip ending)
        {
            explorationMusic = exploration;
            endingMusic = ending;
        }

        public void SetFootstep(AudioClip clip)
        {
            footstep = clip;
        }

        public void SetEffect(SoundEffect effect, AudioClip clip)
        {
            switch (effect)
            {
                case SoundEffect.ButtonPress: buttonPress = clip; break;
                case SoundEffect.Hover: hover = clip; break;
                case SoundEffect.DoorOpen: doorOpen = clip; break;
                case SoundEffect.DiceShake: diceShake = clip; break;
                case SoundEffect.OxygenGain: oxygenGain = clip; break;
                case SoundEffect.OxygenLoss: oxygenLoss = clip; break;
            }
        }

        private GameAudioPlayer CreatePlayer()
        {
            var playerObject = new GameObject(nameof(GameAudioPlayer));
            if (Application.isPlaying)
                DontDestroyOnLoad(playerObject);
            return playerObject.AddComponent<GameAudioPlayer>();
        }
    }
}
