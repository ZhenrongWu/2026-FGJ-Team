using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FGJ.LiarDice.UI
{
    [Serializable]
    public sealed class LiarDiceHudParts
    {
        public OxygenGauge playerOxygen;
        public OxygenGauge monsterOxygen;
        public Text statusText;
        public Text errorText;
        public Button believeButton;
        public Button bluffButton;
        public InputField quantityInput;
        public InputField faceInput;
        public Button submitButton;
        public Button continueButton;
        public Text continueLabel;
        public MatchLogView logView;
        public ItemBarView playerItems;
        public ItemBarView monsterItems;
    }

    public sealed class LiarDiceHud : MonoBehaviour
    {
        [SerializeField] private LiarDiceHudParts parts;

        private readonly LiarDiceText _text = new LiarDiceText();
        private readonly List<string> _itemLabels = new List<string>();
        private bool _listenersWired;
        private ItemBarView _wiredItemBar;

        public event Action BelieveClicked;
        public event Action BluffClicked;
        public event Action<string, string> BidSubmitted;
        public event Action ContinueClicked;
        public event Action<int> ItemClicked;

        public LiarDiceHudParts Parts => parts;
        public MatchLogView LogView => parts.logView;
        public OxygenGauge PlayerOxygen => parts.playerOxygen;
        public OxygenGauge MonsterOxygen => parts.monsterOxygen;
        public string StatusMessage => parts.statusText.text;
        public string ErrorMessage => parts.errorText.text;
        public string ContinueLabel => parts.continueLabel.text;
        public bool IsContinueVisible => parts.continueButton.gameObject.activeSelf;
        public bool IsBelieveEnabled => parts.believeButton.interactable;
        public bool IsBluffEnabled => parts.bluffButton.interactable;
        public bool IsBidInputEnabled => parts.quantityInput.interactable;
        public ItemBarView PlayerItems => parts.playerItems;
        public ItemBarView MonsterItems => parts.monsterItems;

        public void SetParts(LiarDiceHudParts hudParts)
        {
            parts = hudParts;
            WireListeners();
        }

        private void Awake()
        {
            WireListeners();
        }

        private void OnDisable()
        {
            if (parts == null)
                return;
            ReleaseFocus(parts.quantityInput);
            ReleaseFocus(parts.faceInput);
        }

        private void ReleaseFocus(InputField input)
        {
            if (input == null)
                return;
            input.DeactivateInputField();
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == input.gameObject)
                EventSystem.current.SetSelectedGameObject(null);
        }

        private void WireListeners()
        {
            if (parts == null)
                return;
            WireItemBar();
            if (_listenersWired || parts.believeButton == null)
                return;

            parts.believeButton.onClick.AddListener(() => BelieveClicked?.Invoke());
            parts.bluffButton.onClick.AddListener(() => BluffClicked?.Invoke());
            parts.submitButton.onClick.AddListener(SubmitBid);
            parts.continueButton.onClick.AddListener(() => ContinueClicked?.Invoke());
            _listenersWired = true;
        }

        private void WireItemBar()
        {
            if (parts.playerItems == null || _wiredItemBar == parts.playerItems)
                return;
            if (_wiredItemBar != null)
                _wiredItemBar.SlotClicked -= OnItemSlotClicked;
            _wiredItemBar = parts.playerItems;
            _wiredItemBar.SlotClicked += OnItemSlotClicked;
        }

        private void OnItemSlotClicked(int index)
        {
            ItemClicked?.Invoke(index);
        }

        public void ShowItems(LiarDiceMatch match, bool playerCanUse)
        {
            if (parts.playerItems != null)
            {
                _itemLabels.Clear();
                foreach (var item in match.GetItems(Side.Player))
                    _itemLabels.Add(_text.ItemName(item));
                parts.playerItems.Show(_itemLabels, playerCanUse);
            }

            if (parts.monsterItems != null)
            {
                parts.monsterItems.gameObject.SetActive(match.Settings.ItemCount > 0);
                _itemLabels.Clear();
                for (var i = 0; i < match.GetItems(Side.Monster).Count; i++)
                    _itemLabels.Add(LiarDiceText.HiddenItemLabel);
                parts.monsterItems.Show(_itemLabels, false);
            }
        }

        public void SubmitBid()
        {
            if (IsBidInputEnabled)
                BidSubmitted?.Invoke(parts.quantityInput.text, parts.faceInput.text);
        }

        public void ApplyPanelState(BidPanelState state)
        {
            parts.believeButton.interactable = state.BelieveEnabled;
            parts.bluffButton.interactable = state.BluffEnabled;
            parts.quantityInput.interactable = state.BidInputEnabled;
            parts.faceInput.interactable = state.BidInputEnabled;
            parts.submitButton.interactable = state.BidInputEnabled;
            parts.continueButton.gameObject.SetActive(state.ContinueVisible);
            parts.believeButton.gameObject.SetActive(!state.ContinueVisible);
            parts.bluffButton.gameObject.SetActive(!state.ContinueVisible);
        }

        public void ShowOxygen(LiarDiceMatch match)
        {
            parts.playerOxygen.Show(match.PlayerOxygen, match.GetMaxOxygen(Side.Player));
            parts.monsterOxygen.Show(match.MonsterOxygen, match.GetMaxOxygen(Side.Monster));
        }

        public void SetStatus(string message)
        {
            parts.statusText.text = message;
        }

        public void SetContinueLabel(string label)
        {
            parts.continueLabel.text = label;
        }

        public void ShowError(string message)
        {
            parts.errorText.text = message;
        }

        public void ClearError()
        {
            parts.errorText.text = string.Empty;
        }

        public void ClearBidInput()
        {
            parts.quantityInput.text = string.Empty;
            parts.faceInput.text = string.Empty;
        }

        public void FocusQuantity()
        {
            Focus(parts.quantityInput);
        }

        public void CycleInputFocus()
        {
            Focus(parts.quantityInput.isFocused ? parts.faceInput : parts.quantityInput);
        }

        private void Focus(InputField input)
        {
            if (!input.interactable)
                return;
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(input.gameObject);
            input.ActivateInputField();
        }
    }
}
