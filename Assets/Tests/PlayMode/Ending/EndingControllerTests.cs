using System.Collections;
using FGJ.Ending;
using FGJ.Tests.PlayMode.Exploration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace FGJ.Tests.PlayMode.Ending
{
    public class EndingControllerTests
    {
        private RecordingSceneRouter _router;
        private GameObject _root;
        private EndingController _ending;

        [SetUp]
        public void SetUp()
        {
            _router = ScriptableObject.CreateInstance<RecordingSceneRouter>();
            _root = new GameObject("Ending");
            var button = new GameObject("QuitButton", typeof(RectTransform), typeof(Image), typeof(Button));
            button.transform.SetParent(_root.transform);
            _root.SetActive(false);
            _ending = _root.AddComponent<EndingController>();
            _ending.Configure(_router, button.GetComponent<Button>());
            _root.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_root);
            Object.Destroy(_router);
        }

        [UnityTest]
        public IEnumerator QuitButton_Clicked_QuitsGame()
        {
            yield return null;

            _ending.QuitButton.onClick.Invoke();

            Assert.AreEqual(1, _router.QuitRequests);
            Assert.IsFalse(_ending.QuitButton.interactable);
            CollectionAssert.IsEmpty(_router.LoadedScenes);
        }

        [UnityTest]
        public IEnumerator QuitButton_ClickedTwice_QuitsOnlyOnce()
        {
            yield return null;

            _ending.QuitButton.onClick.Invoke();
            _ending.QuitGame();

            Assert.AreEqual(1, _router.QuitRequests);
        }
    }
}
