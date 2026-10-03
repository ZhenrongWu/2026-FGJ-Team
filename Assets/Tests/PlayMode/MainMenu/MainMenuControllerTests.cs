using System.Collections;
using System.Collections.Generic;
using FGJ.Tests.PlayMode.Exploration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace FGJ.Tests.PlayMode.MainMenu
{
    public class MainMenuControllerTests
    {
        private const int SampleRate = 44100;
        private const float ClickSoundSeconds = 0.2f;

        private readonly List<Object> _created = new List<Object>();
        private RecordingSceneRouter _router;
        private RecordingMainMenuController _menu;
        private Button _start;
        private Button _exit;
        private AudioClip _hover;

        [SetUp]
        public void SetUp()
        {
            _router = Track(ScriptableObject.CreateInstance<RecordingSceneRouter>());
            _hover = Track(CreateClip("Hover", 0.05f));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var created in _created)
                Object.Destroy(created);
            _created.Clear();
        }

        private T Track<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }

        private static AudioClip CreateClip(string clipName, float seconds)
        {
            return AudioClip.Create(clipName, Mathf.RoundToInt(seconds * SampleRate), 1, SampleRate, false);
        }

        private IEnumerator BuildMenu(AudioClip clickSound)
        {
            var canvas = Track(new GameObject("Canvas"));
            _start = CreateButton(canvas, "StartButton");
            _exit = CreateButton(canvas, "ExitButton");
            _menu = canvas.AddComponent<RecordingMainMenuController>();
            _menu.Configure(_router, _start, _exit, _hover, clickSound);
            yield return null;
        }

        private static Button CreateButton(GameObject parent, string buttonName)
        {
            var buttonObject = new GameObject(buttonName);
            buttonObject.transform.SetParent(parent.transform, false);
            return buttonObject.AddComponent<Button>();
        }

        [UnityTest]
        public IEnumerator StartClicked_WithoutClickSound_GoesToExploration()
        {
            yield return BuildMenu(null);

            _start.onClick.Invoke();
            yield return null;

            CollectionAssert.AreEqual(new[] { _router.ExplorationScene }, _router.LoadedScenes);
            Assert.AreEqual(0, _router.QuitRequests);
        }

        [UnityTest]
        public IEnumerator ExitClicked_WithoutClickSound_RequestsQuitWithoutLoadingScene()
        {
            yield return BuildMenu(null);

            _exit.onClick.Invoke();
            yield return null;

            Assert.AreEqual(1, _router.QuitRequests);
            CollectionAssert.IsEmpty(_router.LoadedScenes);
        }

        [UnityTest]
        public IEnumerator StartClicked_WithClickSound_DisablesButtonsAndWaitsForSoundBeforeLoading()
        {
            var click = Track(CreateClip("Click", ClickSoundSeconds));
            yield return BuildMenu(click);

            _start.onClick.Invoke();

            CollectionAssert.AreEqual(new[] { click }, _menu.PlayedSounds);
            Assert.IsFalse(_start.interactable);
            Assert.IsFalse(_exit.interactable);
            CollectionAssert.IsEmpty(_router.LoadedScenes);

            yield return new WaitForSecondsRealtime(ClickSoundSeconds + 0.1f);

            CollectionAssert.AreEqual(new[] { _router.ExplorationScene }, _router.LoadedScenes);
        }

        [UnityTest]
        public IEnumerator PointerEnterButton_PlaysHoverSound()
        {
            yield return BuildMenu(null);

            ExecuteEvents.Execute(_exit.gameObject, new PointerEventData(null), ExecuteEvents.pointerEnterHandler);

            CollectionAssert.AreEqual(new[] { _hover }, _menu.PlayedSounds);
            Assert.IsTrue(_exit.interactable);
        }
    }
}
