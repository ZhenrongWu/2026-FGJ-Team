using FGJ.Audio;
using FGJ.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace FGJ.Ending
{
    public sealed class EndingController : MonoBehaviour
    {
        [SerializeField] private SceneRouter router;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameAudio gameAudio;

        private bool _quitRequested;

        public Button QuitButton => quitButton;

        public void Configure(SceneRouter sceneRouter, Button quit)
        {
            router = sceneRouter;
            quitButton = quit;
        }

        public void SetAudio(GameAudio audio)
        {
            gameAudio = audio;
        }

        private void Start()
        {
            if (gameAudio != null)
                gameAudio.PlayEndingMusic();
            quitButton.onClick.AddListener(QuitGame);
        }

        public void QuitGame()
        {
            if (_quitRequested)
                return;
            _quitRequested = true;
            quitButton.interactable = false;
            if (gameAudio != null)
                gameAudio.Play(SoundEffect.ButtonPress);
            router.QuitGame();
        }
    }
}
