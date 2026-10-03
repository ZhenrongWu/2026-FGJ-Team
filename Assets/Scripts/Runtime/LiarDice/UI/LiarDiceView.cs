using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FGJ.LiarDice.UI
{
    [Serializable]
    public sealed class LiarDiceViewParts
    {
        public Text monsterNameText;
        public Text monsterOxygenText;
        public Text monsterLineText;
        public RectTransform monsterDiceRow;
        public Text currentBidText;
        public Text wildStatusText;
        public Text turnStatusText;
        public RectTransform playerDiceRow;
        public Text playerOxygenText;
        public GameObject inputPanel;
        public InputField quantityInput;
        public InputField faceInput;
        public Button raiseButton;
        public Button challengeButton;
        public Text errorText;
        public GameObject resultPanel;
        public Text resultText;
        public Button continueButton;
        public Text continueLabel;
    }

    public sealed class LiarDiceView : MonoBehaviour
    {
        [SerializeField] private LiarDiceViewParts parts;
        [SerializeField] private DieSlotView diePrefab;
        [SerializeField] private LiarDicePalette palette = new LiarDicePalette();

        private readonly List<DieSlotView> _monsterDice = new List<DieSlotView>();
        private readonly List<DieSlotView> _playerDice = new List<DieSlotView>();
        private bool _listenersWired;

        public event Action<string, string> BidSubmitted;
        public event Action ChallengeRequested;
        public event Action ContinueRequested;

        public string QuantityText => parts.quantityInput.text;
        public string FaceText => parts.faceInput.text;
        public string ErrorMessage => parts.errorText.text;
        public string CurrentBidLabel => parts.currentBidText.text;
        public string MonsterLine => parts.monsterLineText.text;
        public string MonsterName => parts.monsterNameText.text;
        public string ResultMessage => parts.resultText.text;
        public string ContinueLabel => parts.continueLabel.text;
        public bool IsResultVisible => parts.resultPanel.activeSelf;
        public bool IsInputInteractable => parts.quantityInput.interactable;

        public IReadOnlyList<DieSlotView> PlayerDice => _playerDice;
        public IReadOnlyList<DieSlotView> MonsterDice => _monsterDice;

        public void SetParts(LiarDiceViewParts viewParts, DieSlotView dieSlotPrefab)
        {
            parts = viewParts;
            diePrefab = dieSlotPrefab;
            WireListeners();
        }

        private void Awake()
        {
            WireListeners();
        }

        private void WireListeners()
        {
            if (_listenersWired || parts == null || parts.raiseButton == null)
                return;

            parts.raiseButton.onClick.AddListener(SubmitBid);
            parts.challengeButton.onClick.AddListener(() => ChallengeRequested?.Invoke());
            parts.continueButton.onClick.AddListener(() => ContinueRequested?.Invoke());
            _listenersWired = true;
        }

        public void SubmitBid()
        {
            BidSubmitted?.Invoke(QuantityText, FaceText);
        }

        public void Render(LiarDiceMatch match, bool revealMonsterDice)
        {
            var settings = match.Settings;
            parts.monsterOxygenText.text = LiarDiceText.Oxygen("怪物", match.MonsterOxygen, settings.MonsterOxygen);
            parts.playerOxygenText.text = LiarDiceText.Oxygen("你的", match.PlayerOxygen, settings.PlayerOxygen);
            parts.currentBidText.text = LiarDiceText.CurrentBid(match.CurrentBid, match.CurrentBidder);
            parts.wildStatusText.text = LiarDiceText.WildStatus(match.WildActive);
            parts.wildStatusText.color = palette.WildStatusColor(match.WildActive);
            parts.turnStatusText.text = LiarDiceText.TurnStatus(match);

            var highlightFace = revealMonsterDice ? match.CurrentBid?.Face : null;
            RenderDice(match.Rules, _playerDice, parts.playerDiceRow, match.GetDice(Side.Player), false, highlightFace,
                match.WildActive);
            RenderDice(match.Rules, _monsterDice, parts.monsterDiceRow, match.GetDice(Side.Monster),
                !revealMonsterDice, highlightFace, match.WildActive);

            var playerCanAct = match.Phase == MatchPhase.Bidding && match.CurrentTurn == Side.Player;
            parts.quantityInput.interactable = playerCanAct && match.CanRaise;
            parts.faceInput.interactable = playerCanAct && match.CanRaise;
            parts.raiseButton.interactable = playerCanAct && match.CanRaise;
            parts.challengeButton.interactable = playerCanAct && match.CanChallenge;
        }

        public void SetMonsterName(string monsterName)
        {
            parts.monsterNameText.text = monsterName;
        }

        public void SetMonsterLine(string line)
        {
            parts.monsterLineText.text = line;
        }

        public void ShowError(string message)
        {
            parts.errorText.text = message;
        }

        public void ClearError()
        {
            parts.errorText.text = string.Empty;
        }

        public void ClearInput()
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

        public void ShowResult(string message, string continueLabel)
        {
            parts.resultText.text = message;
            parts.continueLabel.text = continueLabel;
            parts.resultPanel.SetActive(true);
        }

        public void HideResult()
        {
            parts.resultPanel.SetActive(false);
        }

        private static void Focus(InputField input)
        {
            if (!input.interactable)
                return;
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(input.gameObject);
            input.ActivateInputField();
        }

        private void RenderDice(ILiarDiceRules rules, List<DieSlotView> slots, RectTransform row,
            IReadOnlyList<int> dice, bool hidden, int? highlightFace, bool wildActive)
        {
            while (slots.Count < dice.Count)
                slots.Add(Instantiate(diePrefab, row));

            for (var i = 0; i < slots.Count; i++)
            {
                var visible = i < dice.Count;
                slots[i].gameObject.SetActive(visible);
                if (!visible)
                    continue;

                var highlighted = !hidden && highlightFace.HasValue &&
                                  rules.CountMatching(new[] { dice[i] }, highlightFace.Value, wildActive) > 0;
                slots[i].Show(dice[i], hidden, highlighted, palette);
            }
        }
    }
}
