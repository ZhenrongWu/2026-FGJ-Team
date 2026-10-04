using System;
using System.Collections.Generic;
using FGJ.Audio;
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
        public Text itemTooltip;
    }

    public sealed class LiarDiceHud : MonoBehaviour
    {
        [SerializeField] private LiarDiceHudParts parts;
        [SerializeField] private GameAudio gameAudio;
        [SerializeField] private ItemIconSet itemIcons;

        private readonly LiarDiceText _text = new LiarDiceText();
        private readonly List<ItemSlot> _itemSlots = new List<ItemSlot>();
        private readonly List<ItemType> _playerItems = new List<ItemType>();
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
        public ItemIconSet ItemIcons => itemIcons;
        public string ItemTooltip => parts.itemTooltip != null ? parts.itemTooltip.text : string.Empty;

        public void SetAudio(GameAudio audio)
        {
            gameAudio = audio;
        }

        public void SetItemIcons(ItemIconSet icons)
        {
            itemIcons = icons;
        }

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

            parts.believeButton.onClick.AddListener(PlayButtonPress);
            parts.believeButton.onClick.AddListener(() => BelieveClicked?.Invoke());
            parts.bluffButton.onClick.AddListener(PlayButtonPress);
            parts.bluffButton.onClick.AddListener(() => BluffClicked?.Invoke());
            parts.submitButton.onClick.AddListener(PlayButtonPress);
            parts.submitButton.onClick.AddListener(SubmitBid);
            parts.continueButton.onClick.AddListener(PlayButtonPress);
            parts.continueButton.onClick.AddListener(() => ContinueClicked?.Invoke());
            _listenersWired = true;
        }

        private void WireItemBar()
        {
            if (parts.playerItems == null || _wiredItemBar == parts.playerItems)
                return;
            if (_wiredItemBar != null)
            {
                _wiredItemBar.SlotClicked -= OnItemSlotClicked;
                _wiredItemBar.SlotHovered -= ShowItemTooltip;
                _wiredItemBar.HoverEnded -= ClearItemTooltip;
            }
            _wiredItemBar = parts.playerItems;
            _wiredItemBar.SlotClicked += OnItemSlotClicked;
            _wiredItemBar.SlotHovered += ShowItemTooltip;
            _wiredItemBar.HoverEnded += ClearItemTooltip;
        }

        private void ShowItemTooltip(int index)
        {
            if (parts.itemTooltip != null && index < _playerItems.Count)
                parts.itemTooltip.text = _text.ItemTooltip(_playerItems[index]);
        }

        private void ClearItemTooltip()
        {
            if (parts.itemTooltip != null)
                parts.itemTooltip.text = string.Empty;
        }

        private void OnItemSlotClicked(int index)
        {
            PlayButtonPress();
            ItemClicked?.Invoke(index);
        }

        private void PlayButtonPress()
        {
            if (gameAudio != null)
                gameAudio.Play(SoundEffect.ButtonPress);
        }

        public void ShowItems(LiarDiceMatch match, bool playerCanUse)
        {
            var levelHasItems = match.Settings.ItemCount > 0;
            ClearItemTooltip();
            _playerItems.Clear();
            _playerItems.AddRange(match.GetItems(Side.Player));
            if (parts.playerItems != null)
            {
                parts.playerItems.gameObject.SetActive(levelHasItems);
                _itemSlots.Clear();
                foreach (var item in _playerItems)
                    _itemSlots.Add(new ItemSlot(_text.ItemName(item), IconFor(item)));
                parts.playerItems.Show(_itemSlots, playerCanUse);
            }

            if (parts.monsterItems != null)
            {
                parts.monsterItems.gameObject.SetActive(levelHasItems);
                _itemSlots.Clear();
                for (var i = 0; i < match.GetItems(Side.Monster).Count; i++)
                    _itemSlots.Add(new ItemSlot(LiarDiceText.HiddenItemLabel, null));
                parts.monsterItems.Show(_itemSlots, false);
            }
        }

        private Sprite IconFor(ItemType item) => itemIcons != null ? itemIcons.IconFor(item) : null;

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
