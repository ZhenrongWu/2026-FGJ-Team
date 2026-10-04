using System.Collections;
using FGJ.Flow;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FGJ.MainMenu
{
    [RequireComponent(typeof(AudioSource))]
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private SceneRouter router;
        [SerializeField] private Button startButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;

        private AudioSource audioSource;

        public void Configure(SceneRouter sceneRouter, Button start, Button exit, AudioClip hover, AudioClip click)
        {
            router = sceneRouter;
            startButton = start;
            exitButton = exit;
            hoverSound = hover;
            clickSound = click;
        }

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            BindButton(startButton, router.GoToExploration);
            BindButton(exitButton, router.QuitGame);
        }

        private void BindButton(Button button, UnityAction onConfirmed)
        {
            button.onClick.AddListener(() => StartCoroutine(PlayClickThenRun(onConfirmed)));
            PlayHoverSoundOnPointerEnter(button);
        }

        private void PlayHoverSoundOnPointerEnter(Button button)
        {
            var trigger = button.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<EventTrigger>();

            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entry.callback.AddListener(_ => PlayHoverSoundIfInteractable(button));
            trigger.triggers.Add(entry);
        }

        private void PlayHoverSoundIfInteractable(Button button)
        {
            if (button.IsInteractable())
                PlaySound(hoverSound);
        }

        private IEnumerator PlayClickThenRun(UnityAction onConfirmed)
        {
            SetButtonsInteractable(false);
            PlaySound(clickSound);

            if (clickSound != null)
                yield return new WaitForSecondsRealtime(clickSound.length);

            onConfirmed.Invoke();
        }

        private void SetButtonsInteractable(bool interactable)
        {
            startButton.interactable = interactable;
            exitButton.interactable = interactable;
        }

        protected virtual void PlaySound(AudioClip clip)
        {
            if (clip != null)
                audioSource.PlayOneShot(clip);
        }
    }
}
