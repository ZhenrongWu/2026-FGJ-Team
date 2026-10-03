using UnityEngine;
using UnityEngine.SceneManagement;

namespace FGJ.Flow
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        private void Start()
        {
            GameSession.Reset();
            SceneManager.LoadScene(SceneFlow.StartScene());
        }
    }
}
