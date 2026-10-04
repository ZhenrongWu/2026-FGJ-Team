using FGJ.Audio;
using UnityEditor;
using UnityEngine;

namespace FGJ.Editor
{
    public static class AudioAssets
    {
        public const string Folder = "Assets/Data/Audio";
        public const string GameAudioPath = Folder + "/GameAudio.asset";
        public const string MusicFolder = "Assets/Audio/Music";
        public const string EffectsFolder = "Assets/Audio/SFX";

        public static GameAudio GameAudio
        {
            get
            {
                var audio = AssetDatabase.LoadAssetAtPath<GameAudio>(GameAudioPath);
                return audio != null ? audio : CreateGameAudio();
            }
        }

        public static string LevelMusicPath(int level) => $"{MusicFolder}/BGM_Level{level}.mp3";

        [MenuItem("FGJ/Data/Assign Audio Clips")]
        public static void AssignAll()
        {
            AssignClips(GameAudio);
            for (var level = 1; level <= ExplorationSceneMenu.LevelCount; level++)
            {
                var config = LiarDiceSceneMenu.LevelConfig(level);
                if (config == null)
                    continue;
                config.SetMusic(Load(LevelMusicPath(level)));
                EditorUtility.SetDirty(config);
            }
            AssetDatabase.SaveAssets();
        }

        private static GameAudio CreateGameAudio()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/Data", "Audio");
            var audio = ScriptableObject.CreateInstance<GameAudio>();
            AssetDatabase.CreateAsset(audio, GameAudioPath);
            AssignClips(audio);
            AssetDatabase.SaveAssets();
            return audio;
        }

        private static void AssignClips(GameAudio audio)
        {
            audio.SetMusic(Load($"{MusicFolder}/BGM_Exploration.mp3"), Load($"{MusicFolder}/BGM_Ending.mp3"));
            audio.SetFootstep(Load($"{EffectsFolder}/Player/SFX_Footstep.mp3"));
            audio.SetEffect(SoundEffect.ButtonPress, Load($"{EffectsFolder}/UI/SFX_ButtonPress.mp3"));
            audio.SetEffect(SoundEffect.Hover, Load($"{EffectsFolder}/UI/SFX_Hover.wav"));
            audio.SetEffect(SoundEffect.DoorOpen, Load($"{EffectsFolder}/Building/SFX_DoorOpen.mp3"));
            audio.SetEffect(SoundEffect.DiceShake, Load($"{EffectsFolder}/Dice/SFX_DiceShake.mp3"));
            audio.SetEffect(SoundEffect.OxygenGain, Load($"{EffectsFolder}/Oxygen/SFX_OxygenGain.mp3"));
            audio.SetEffect(SoundEffect.OxygenLoss, Load($"{EffectsFolder}/Oxygen/SFX_OxygenLoss.mp3"));
            EditorUtility.SetDirty(audio);
        }

        private static AudioClip Load(string path) => AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
}
