using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FGJ
{
    [RequireComponent(typeof(AudioSource))]
    public class StartToSample : MonoBehaviour
    {
        [Header("場景設定")]
        [SerializeField, Tooltip("Start 按鈕要載入的場景名稱")]
        private string targetSceneName = "SampleScene";

        [Header("音效設定")]
        [SerializeField, Tooltip("滑鼠滑過按鈕時的音效")]
        private AudioClip hoverSound;

        [SerializeField, Tooltip("按下按鈕時的音效")]
        private AudioClip clickSound;

        private AudioSource audioSource;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();

            // 自動尋找子物件中的按鈕並綁定事件
            SetupButton("StartButton", OnStartButtonClicked);
            SetupButton("ExitButton", OnExitButtonClicked);
        }

        private void SetupButton(string buttonName, UnityEngine.Events.UnityAction onClick)
        {
            Transform child = transform.Find(buttonName);
            if (child == null)
            {
                Debug.LogWarning($"[StartToSample] 找不到名為 '{buttonName}' 的子物件");
                return;
            }

            Button button = child.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogWarning($"[StartToSample] '{buttonName}' 上找不到 Button 元件");
                return;
            }

            // 綁定點擊事件（播完音效再執行動作）
            button.onClick.AddListener(() => StartCoroutine(ClickAndExecute(onClick)));

            // 綁定滑鼠滑入事件
            AddPointerEnterEvent(child.gameObject);
        }

        private void AddPointerEnterEvent(GameObject target)
        {
            EventTrigger trigger = target.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = target.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entry.callback.AddListener((_) => PlaySound(hoverSound));

            trigger.triggers.Add(entry);
        }

        private IEnumerator ClickAndExecute(UnityEngine.Events.UnityAction action)
        {
            // 防止連點：播放期間停用所有按鈕
            SetAllButtonsInteractable(false);

            PlaySound(clickSound);

            // 等音效播完
            if (clickSound != null)
                yield return new WaitForSeconds(clickSound.length);

            action.Invoke();
        }

        private void SetAllButtonsInteractable(bool interactable)
        {
            foreach (Button btn in GetComponentsInChildren<Button>())
                btn.interactable = interactable;
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
                audioSource.PlayOneShot(clip);
        }

        private void OnStartButtonClicked()
        {
            SceneManager.LoadScene(targetSceneName);
        }

        private void OnExitButtonClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
