using System;
using System.Collections.Generic;
using FGJ.Flow;

namespace FGJ.Tests.EditMode.Exploration
{
    internal sealed class RecordingSceneRouter : SceneRouter
    {
        public readonly List<string> LoadedScenes = new List<string>();
        public Func<string, bool> Availability = _ => true;

        protected override bool CanLoad(string sceneName) => Availability(sceneName);

        protected override void Load(string sceneName) => LoadedScenes.Add(sceneName);
    }
}
