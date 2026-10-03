using System.Collections.Generic;
using FGJ.MainMenu;
using UnityEngine;

namespace FGJ.Tests.PlayMode.MainMenu
{
    internal sealed class RecordingMainMenuController : MainMenuController
    {
        public readonly List<AudioClip> PlayedSounds = new List<AudioClip>();

        protected override void PlaySound(AudioClip clip) => PlayedSounds.Add(clip);
    }
}
