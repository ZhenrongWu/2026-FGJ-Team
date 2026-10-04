using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FGJ.LiarDice.UI
{
    public sealed class ItemBarView : MonoBehaviour
    {
        [SerializeField] private RectTransform slotRoot;
        [SerializeField] private Button slotTemplate;

        private readonly List<Button> _slots = new List<Button>();
        private readonly List<string> _labels = new List<string>();

        public event Action<int> SlotClicked;

        public int VisibleCount { get; private set; }
        public bool IsInteractable { get; private set; }
        public IReadOnlyList<string> Labels => _labels;

        public void Configure(RectTransform root, Button template)
        {
            slotRoot = root;
            slotTemplate = template;
        }

        public void Show(IReadOnlyList<string> labels, bool interactable)
        {
            while (_slots.Count < labels.Count)
                _slots.Add(CreateSlot(_slots.Count));

            _labels.Clear();
            _labels.AddRange(labels);
            VisibleCount = labels.Count;
            IsInteractable = interactable;
            for (var i = 0; i < _slots.Count; i++)
            {
                var visible = i < labels.Count;
                _slots[i].gameObject.SetActive(visible);
                if (!visible)
                    continue;
                _slots[i].interactable = interactable;
                _slots[i].GetComponentInChildren<Text>().text = labels[i];
            }
        }

        public void Click(int index)
        {
            if (IsInteractable && index >= 0 && index < VisibleCount)
                SlotClicked?.Invoke(index);
        }

        private Button CreateSlot(int index)
        {
            var slot = Instantiate(slotTemplate, slotRoot);
            slot.name = $"ItemSlot_{index}";
            slot.gameObject.SetActive(true);
            slot.onClick.AddListener(() => Click(index));
            return slot;
        }
    }
}
