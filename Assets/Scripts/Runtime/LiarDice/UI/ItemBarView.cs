using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FGJ.LiarDice.UI
{
    public sealed class ItemBarView : MonoBehaviour
    {
        public const string IconName = "Icon";

        [SerializeField] private RectTransform slotRoot;
        [SerializeField] private Button slotTemplate;

        private readonly List<Button> _slots = new List<Button>();
        private readonly List<string> _labels = new List<string>();
        private readonly List<Sprite> _icons = new List<Sprite>();

        public event Action<int> SlotClicked;
        public event Action<int> SlotHovered;
        public event Action HoverEnded;

        public int VisibleCount { get; private set; }
        public bool IsInteractable { get; private set; }
        public IReadOnlyList<string> Labels => _labels;
        public IReadOnlyList<Sprite> Icons => _icons;

        public void Configure(RectTransform root, Button template)
        {
            slotRoot = root;
            slotTemplate = template;
        }

        public void Show(IReadOnlyList<ItemSlot> slots, bool interactable)
        {
            while (_slots.Count < slots.Count)
                _slots.Add(CreateSlot(_slots.Count));

            _labels.Clear();
            _icons.Clear();
            VisibleCount = slots.Count;
            IsInteractable = interactable;
            for (var i = 0; i < _slots.Count; i++)
            {
                var visible = i < slots.Count;
                _slots[i].gameObject.SetActive(visible);
                if (!visible)
                    continue;
                _labels.Add(slots[i].Label);
                _icons.Add(slots[i].Icon);
                _slots[i].interactable = interactable;
                Present(_slots[i], slots[i]);
            }
        }

        private static void Present(Button slot, ItemSlot content)
        {
            var iconTransform = slot.transform.Find(IconName);
            var hasIcon = content.Icon != null && iconTransform != null;
            if (iconTransform != null)
            {
                var icon = iconTransform.GetComponent<Image>();
                icon.sprite = content.Icon;
                icon.enabled = hasIcon;
            }
            slot.GetComponentInChildren<Text>(true).text = hasIcon ? string.Empty : content.Label;
        }

        public void Click(int index)
        {
            if (IsInteractable && index >= 0 && index < VisibleCount)
                SlotClicked?.Invoke(index);
        }

        public void Hover(int index)
        {
            if (index >= 0 && index < VisibleCount)
                SlotHovered?.Invoke(index);
        }

        public void EndHover()
        {
            HoverEnded?.Invoke();
        }

        private Button CreateSlot(int index)
        {
            var slot = Instantiate(slotTemplate, slotRoot);
            slot.name = $"ItemSlot_{index}";
            slot.gameObject.SetActive(true);
            slot.onClick.AddListener(() => Click(index));
            var trigger = slot.gameObject.AddComponent<EventTrigger>();
            AddTrigger(trigger, EventTriggerType.PointerEnter, () => Hover(index));
            AddTrigger(trigger, EventTriggerType.PointerExit, EndHover);
            return slot;
        }

        private static void AddTrigger(EventTrigger trigger, EventTriggerType type, Action callback)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(_ => callback());
            trigger.triggers.Add(entry);
        }
    }
}
