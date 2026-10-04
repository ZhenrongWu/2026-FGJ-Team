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
            var building = progress != null ? progress.CurrentBuilding : null;
            if (controller == null || building == null)
                return;

            controller.UseEncounter(building.GameplayConfig, building.Monster);
            if (controller.Config != null)
                controller.SetPlayerStartOxygen(progress.PlayerOxygenFor(controller.Config.PlayerOxygen));
        }

        private void OnExitRequested(Side winner)
        {
            if (winner != Side.Player)
            {
                router.GoToGameplay();
                return;
            }

            var config = controller.Config;
            var oxygenAfter = config != null ? config.OxygenAfterVictory(controller.Match.PlayerOxygen) : 0;
            progress.CompleteBuilding(true, oxygenAfter);
            if (config != null && config.EndsGame)
            {
                progress.ResetRun();
                router.GoToEnding();
            }
            else
            {
                router.GoToExploration();
            }
        }
    }
}
