using UnityEngine;

namespace FGJ.Flow
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameProgress progress;
        [SerializeField] private SceneRouter router;

        public void Configure(GameProgress gameProgress, SceneRouter sceneRouter)
        {
            progress = gameProgress;
            router = sceneRouter;
        }

        private void Start()
        {
            progress.ResetRun();
            router.GoToStart();
        }
    }
}
