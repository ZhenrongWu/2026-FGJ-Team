using System;
using FGJ.Flow;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FGJ.LiarDice
{
    public sealed class LiarDiceRoomSceneFlow : MonoBehaviour
    {
        [SerializeField] private LiarDiceRoomController controller;
        [SerializeField] private string explorationScene = SceneNames.Exploration;

        private Action<string> _sceneLoader = SceneManager.LoadScene;
        private Func<string, bool> _canLoadScene = SceneFlow.CanLoad;

        public void Configure(LiarDiceRoomController roomController, string sceneAfterWin)
        {
            controller = roomController;
            explorationScene = sceneAfterWin;
        }

        public void SetSceneLoader(Action<string> loader)
        {
            _sceneLoader = loader ?? throw new ArgumentNullException(nameof(loader));
        }

        public void SetSceneAvailability(Func<string, bool> canLoadScene)
        {
            _canLoadScene = canLoadScene ?? throw new ArgumentNullException(nameof(canLoadScene));
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

        private void OnExitRequested(Side winner)
        {
            var playerWon = winner == Side.Player;
            GameSession.CompleteRoom(playerWon);
            _sceneLoader(playerWon ? explorationScene : SceneFlow.StartScene(_canLoadScene));
        }
    }
}
