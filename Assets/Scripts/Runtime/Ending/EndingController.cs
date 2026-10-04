using FGJ.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace FGJ.Ending
{
    public sealed class EndingController : MonoBehaviour
    {
        [SerializeField] private SceneRouter router;
        [SerializeField] private Button quitButton;

        private bool _quitRequested;

        public Button QuitButton => quitButton;

        public void Configure(SceneRouter sceneRouter, Button quit)
        {
            router = sceneRouter;
            quitButton = quit;
        }

        private void Start()
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        public void QuitGame()
        {
            if (_quitRequested)
                return;
            _quitRequested = true;
            quitButton.interactable = false;
            router.QuitGame();
        }
    }
}
