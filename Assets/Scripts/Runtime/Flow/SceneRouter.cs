using UnityEngine;
using UnityEngine.SceneManagement;

namespace FGJ.Flow
{
    [CreateAssetMenu(fileName = "SceneRouter", menuName = "FGJ/Flow/Scene Router")]
    public class SceneRouter : ScriptableObject
    {
        [SerializeField] private string mainMenuScene = SceneNames.MainMenu;
        [SerializeField] private string explorationScene = SceneNames.Exploration;
        [SerializeField] private string gameplayScene = SceneNames.Gameplay;
        [Tooltip("結局 CG 場景，尚未加入 Build Settings 時改回起始場景")]
        [SerializeField] private string endingScene = SceneNames.Ending;

        public string MainMenuScene => mainMenuScene;
        public string ExplorationScene => explorationScene;
        public string GameplayScene => gameplayScene;
        public string EndingScene => endingScene;

        public string StartScene => CanLoad(mainMenuScene) ? mainMenuScene : explorationScene;

        public void GoToStart() => Load(StartScene);
        public void GoToExploration() => Load(explorationScene);
        public void GoToGameplay() => Load(gameplayScene);
        public void GoToEnding() => Load(CanLoad(endingScene) ? endingScene : StartScene);
        public void QuitGame() => Quit();

        protected virtual bool CanLoad(string sceneName) => Application.CanStreamedLevelBeLoaded(sceneName);

        protected virtual void Load(string sceneName) => SceneManager.LoadScene(sceneName);

        protected virtual void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
