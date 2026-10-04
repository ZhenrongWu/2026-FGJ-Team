using System.Collections;
using FGJ.Audio;
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
        private RecordingGameAudio _audio;
        private GameObject _root;
        private EndingController _ending;

        [SetUp]
        public void SetUp()
        {
            _router = ScriptableObject.CreateInstance<RecordingSceneRouter>();
            _audio = RecordingGameAudio.Create();
            _root = new GameObject("Ending");
            var button = new GameObject("QuitButton", typeof(RectTransform), typeof(Image), typeof(Button));
            button.transform.SetParent(_root.transform);
            _root.SetActive(false);
            _ending = _root.AddComponent<EndingController>();
            _ending.Configure(_router, button.GetComponent<Button>());
            _ending.SetAudio(_audio);
            _root.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_root);
            Object.Destroy(_router);
            Object.Destroy(_audio);
        }

        [UnityTest]
        public IEnumerator QuitButton_Clicked_QuitsGame()
        {
            yield return null;

            _ending.QuitButton.onClick.Invoke();

            Assert.AreEqual(1, _router.QuitRequests);
            Assert.IsFalse(_ending.QuitButton.interactable);
            CollectionAssert.IsEmpty(_router.LoadedScenes);
            CollectionAssert.AreEqual(new[] { SoundEffect.ButtonPress }, _audio.Effects);
        }

        [UnityTest]
        public IEnumerator Start_PlaysEndingMusic()
        {
            yield return null;

            CollectionAssert.AreEqual(new[] { _audio.EndingMusic }, _audio.Music);
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
