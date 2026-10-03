using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FGJ.Exploration
{
    public sealed class ExplorationHud : MonoBehaviour
    {
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private Text promptText;
        [SerializeField] private Image screenFader;

        public string PromptMessage => promptRoot.activeSelf ? promptText.text : string.Empty;
        public float FadeAlpha => screenFader != null ? screenFader.color.a : 0f;

        public void Configure(GameObject prompt, Text promptLabel, Image fader)
        {
            promptRoot = prompt;
            promptText = promptLabel;
            screenFader = fader;
        }

        public void ShowPrompt(string message)
        {
            promptText.text = message;
            promptRoot.SetActive(true);
        }

        public void HidePrompt()
        {
            promptRoot.SetActive(false);
        }

        public IEnumerator Fade(float from, float to, float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                SetFade(Mathf.Lerp(from, to, elapsed / duration));
                elapsed += Time.deltaTime;
                yield return null;
            }
            SetFade(to);
        }

        public void SetFade(float alpha)
        {
            if (screenFader == null)
                return;
            var color = screenFader.color;
            color.a = alpha;
            screenFader.color = color;
            screenFader.raycastTarget = alpha > 0.01f;
        }
    }
}
