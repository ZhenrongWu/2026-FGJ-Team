using System;
using System.Collections.Generic;
using FGJ.Flow;

namespace FGJ.Tests.PlayMode.Exploration
{
    internal sealed class RecordingSceneRouter : SceneRouter
    {
        public readonly List<string> LoadedScenes = new List<string>();
        public Func<string, bool> Availability = _ => true;
        public int QuitRequests { get; private set; }

        protected override bool CanLoad(string sceneName) => Availability(sceneName);

        protected override void Load(string sceneName) => LoadedScenes.Add(sceneName);

        protected override void Quit() => QuitRequests++;
    }
}
