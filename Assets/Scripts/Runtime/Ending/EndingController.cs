using FGJ.Audio;
using FGJ.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace FGJ.Ending
{
    public sealed class EndingController : MonoBehaviour
    {
        [SerializeField] private SceneRouter router;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private GameAudio gameAudio;

        private bool _leaveRequested;

        public Button MainMenuButton => mainMenuButton;

        public void Configure(SceneRouter sceneRouter, Button mainMenu)
        {
            router = sceneRouter;
            mainMenuButton = mainMenu;
        }

        public void SetAudio(GameAudio audio)
        {
            gameAudio = audio;
        }

        private void Start()
        {
            if (gameAudio != null)
                gameAudio.PlayEndingMusic();
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        public void ReturnToMainMenu()
        {
            if (_leaveRequested)
                return;
            _leaveRequested = true;
            mainMenuButton.interactable = false;
            if (gameAudio != null)
                gameAudio.Play(SoundEffect.ButtonPress);
            router.GoToStart();
        }
    }
}
