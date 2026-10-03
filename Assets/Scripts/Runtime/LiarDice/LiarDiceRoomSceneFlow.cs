using FGJ.Flow;
using UnityEngine;

namespace FGJ.LiarDice
{
    public sealed class LiarDiceRoomSceneFlow : MonoBehaviour
    {
        [SerializeField] private LiarDiceRoomController controller;
        [SerializeField] private GameProgress progress;
        [SerializeField] private SceneRouter router;

        public void Configure(LiarDiceRoomController roomController, GameProgress gameProgress, SceneRouter sceneRouter)
        {
            controller = roomController;
            progress = gameProgress;
            router = sceneRouter;
        }

        private void Awake()
        {
            ApplyBuildingConfig();
        }

        private void OnEnable()
        {
            if (controller != null)
                controller.ExitRequested += OnExitRequested;
        }

        private void OnDisable()
        {
            if (controller != null)
                controller.ExitRequested -= OnExitRequested;
        }

        private void ApplyBuildingConfig()
        {
            var buildingConfig = progress != null && progress.CurrentBuilding != null
                ? progress.CurrentBuilding.GameplayConfig
                : null;
            if (controller != null && buildingConfig != null)
                controller.UseConfig(buildingConfig);
        }

        private void OnExitRequested(Side winner)
        {
            var playerWon = winner == Side.Player;
            progress.CompleteBuilding(playerWon);
            if (playerWon)
                router.GoToExploration();
            else
                router.GoToStart();
        }
    }
}
