using System;
using System.Collections;
using FGJ.LiarDice.Table;
using FGJ.LiarDice.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace FGJ.LiarDice
{
    public sealed class LiarDiceRoomController : MonoBehaviour
    {
        private const string PlayerSpeaker = "你";
        private const string PlayerChallengeLine = "「吹牛！開！」";

        [SerializeField] private LiarDiceConfig config;
        [SerializeField] private MonsterProfile monster;
        [SerializeField] private LiarDiceHud hud;
        [SerializeField] private DiceTableView table;
        [SerializeField] private MonsterView monsterView;
        [Min(0f)] [SerializeField] private float monsterThinkSeconds = 1.2f;
        [SerializeField] private UnityEvent<Side> matchFinished = new UnityEvent<Side>();

        private readonly MatchLog _log = new MatchLog();
        private readonly LiarDiceText _text = new LiarDiceText();
        private readonly BidPanelRules _panelRules = new BidPanelRules();
        private MonsterAI _monsterAI;
        private Func<IDiceRoller> _rollerFactory;
        private Func<IItemDealer> _dealerFactory;
        private MonsterProfile _fallbackMonster;
        private int _playerStartOxygen;
        private bool _believed;
        private int _roundNumber;

        public event Action<Side> MatchFinished;
        public event Action<Side> ExitRequested;

        public LiarDiceMatch Match { get; private set; }
        public LiarDiceConfig Config => config;
        public LiarDiceHud Hud => hud;
        public DiceTableView Table => table;
        public MonsterView MonsterView => monsterView;
        public MatchLog Log => _log;
        public bool IsBusy { get; private set; }
        public bool HasBelieved => _believed;
        public bool IsPlayerTurn => Match != null && !IsBusy && Match.Phase == MatchPhase.Bidding &&
                                    Match.CurrentTurn == Side.Player;
        public MonsterProfile Monster => monster != null ? monster : FallbackMonster;

        private MonsterProfile FallbackMonster => _fallbackMonster != null
            ? _fallbackMonster
            : _fallbackMonster = ScriptableObject.CreateInstance<MonsterProfile>();

        private bool EndsGame => config != null && config.EndsGame;

        public void Configure(LiarDiceHud roomHud, DiceTableView diceTable, MonsterView roomMonsterView)
        {
            hud = roomHud;
            table = diceTable;
            monsterView = roomMonsterView;
        }

        public void UseEncounter(LiarDiceConfig roomConfig, MonsterProfile roomMonster)
        {
            if (roomConfig != null)
                config = roomConfig;
            if (roomMonster != null)
                monster = roomMonster;
        }

        public void SetPlayerStartOxygen(int oxygen)
        {
            _playerStartOxygen = Mathf.Max(0, oxygen);
        }

        public void SetMonsterThinkSeconds(float seconds)
        {
            monsterThinkSeconds = Mathf.Max(0f, seconds);
        }

        private void Start()
        {
            if (Match != null || config == null)
                return;

            var settings = _playerStartOxygen > 0
                ? config.ToMatchSettings(_playerStartOxygen)
                : config.ToMatchSettings();
            Begin(settings, () => new RandomDiceRoller(),
                new MonsterAI(Monster.ToAIProfile(), rules: config.ToRules()));
        }

        private void OnDestroy()
        {
            if (_fallbackMonster != null)
                Destroy(_fallbackMonster);
            UnbindHud();
        }

        public void Begin(MatchSettings settings, Func<IDiceRoller> rollerFactory, MonsterAI monsterAI,
            Func<IItemDealer> dealerFactory = null)
        {
            _rollerFactory = rollerFactory ?? throw new ArgumentNullException(nameof(rollerFactory));
            _monsterAI = monsterAI ?? throw new ArgumentNullException(nameof(monsterAI));
            _dealerFactory = dealerFactory ?? (() => new RandomItemDealer());
            StopAllCoroutines();
            IsBusy = false;

            UnbindHud();
            hud.BelieveClicked += Believe;
            hud.BluffClicked += ChallengeAsPlayer;
            hud.BidSubmitted += OnBidSubmitted;
            hud.ContinueClicked += Continue;
            hud.ItemClicked += OnItemClicked;

            Match = new LiarDiceMatch(settings, _rollerFactory(), _dealerFactory());
            Match.Start();
            _roundNumber = 0;
            _log.Clear();
            hud.LogView.Bind(_log);
            hud.ClearError();
            hud.ClearBidInput();
            if (monsterView != null)
                monsterView.Show(Monster);
            _log.Add(_text.Speech(Monster.DisplayName, Monster.Greeting), LogKind.MonsterSpeech);
            StartRound();
        }

        public void Believe()
        {
            if (!IsPlayerTurn)
            {
                hud.ShowError(LiarDiceText.NotPlayerTurn);
                return;
            }
            if (!Match.CurrentBid.HasValue || !Match.CanRaise)
                return;

            _believed = true;
            hud.ClearError();
            Refresh();
            hud.FocusQuantity();
        }

        public BidInputResult SubmitPlayerBid(string quantityText, string faceText)
        {
            if (!IsPlayerTurn)
            {
                hud.ShowError(LiarDiceText.NotPlayerTurn);
                return BidInputResult.Fail(LiarDiceText.NotPlayerTurn);
            }

            var input = new BidInputValidator(Match.Rules).Validate(quantityText, faceText, Match.CurrentBid,
                Match.TotalDiceKnownTo(Side.Player));
            if (!input.IsValid)
            {
                hud.ShowError(input.ErrorMessage);
                return input;
            }

            var answeringBid = Match.CurrentBid.HasValue && !Match.MustRaise;
            Match.PlaceBid(Side.Player, input.Bid);
            _log.Add(_text.PlayerBid(input.Bid, answeringBid), LogKind.PlayerAction);
            LogSkippedTurn();
            _believed = false;
            hud.ClearError();
            hud.ClearBidInput();
            AdvanceTurn();
            return input;
        }

        public void ChallengeAsPlayer()
        {
            if (!IsPlayerTurn)
            {
                hud.ShowError(LiarDiceText.NotPlayerTurn);
                return;
            }
            if (!Match.CanChallenge)
            {
                hud.ShowError(LiarDiceText.NothingToChallenge);
                return;
            }

            hud.ClearError();
            _log.Add(_text.Speech(PlayerSpeaker, PlayerChallengeLine), LogKind.PlayerAction);
            StartCoroutine(Resolve(Match.Challenge(Side.Player)));
        }

        public ItemUseResult UsePlayerItem(ItemType item)
        {
            if (!IsPlayerTurn)
            {
                hud.ShowError(LiarDiceText.NotPlayerTurn);
                return ItemUseResult.NotHeld;
            }

            var result = Match.UseItem(Side.Player, item);
            if (result == ItemUseResult.AlreadyActive)
                hud.ShowError(LiarDiceText.ItemAlreadyActive);
            if (result != ItemUseResult.Used)
                return result;

            hud.ClearError();
            ApplyItemEffect(Side.Player, item);
            if (item == ItemType.Reroll)
                StartCoroutine(PlayReroll());
            else
                Refresh();
            return result;
        }

        public void Continue()
        {
            if (Match == null || IsBusy)
                return;

            switch (Match.Phase)
            {
                case MatchPhase.RoundOver:
                    Match.NextRound();
                    StartRound();
                    break;
                case MatchPhase.MatchOver:
                    if (ExitRequested != null)
                        ExitRequested.Invoke(Match.Winner.Value);
                    else
                        Begin(Match.Settings, _rollerFactory, _monsterAI, _dealerFactory);
                    break;
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !IsPlayerTurn)
                return;

            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
                hud.SubmitBid();
            else if (keyboard.tabKey.wasPressedThisFrame)
                hud.CycleInputFocus();
        }

        private void OnBidSubmitted(string quantityText, string faceText)
        {
            SubmitPlayerBid(quantityText, faceText);
        }

        private void OnItemClicked(int index)
        {
            if (Match == null)
                return;
            var items = Match.GetItems(Side.Player);
            if (index >= 0 && index < items.Count)
                UsePlayerItem(items[index]);
        }

        private void StartRound()
        {
            _roundNumber++;
            _believed = false;
            _log.Add(_text.RoundStart(_roundNumber, Match.Settings.FirstTurn), LogKind.System);
            var peek = Match.GetPeek(Side.Player);
            if (peek.HasValue)
                _log.Add(_text.PeekReveal(peek.Value), LogKind.System);
            LogSkippedTurn();
            StartCoroutine(PlayRoundStart());
        }

        private IEnumerator PlayRoundStart()
        {
            IsBusy = true;
            Refresh();
            yield return table.PlayRoundStart(Match);
            IsBusy = false;
            AdvanceTurn();
        }

        private void AdvanceTurn()
        {
            Refresh();
            if (Match.Phase != MatchPhase.Bidding)
                return;

            if (Match.CurrentTurn == Side.Monster)
                StartCoroutine(PlayMonsterTurn());
            else if (!Match.CurrentBid.HasValue || Match.MustRaise)
                hud.FocusQuantity();
        }

        private IEnumerator PlayMonsterTurn()
        {
            if (monsterThinkSeconds > 0f)
                yield return new WaitForSeconds(monsterThinkSeconds);
            else
                yield return null;

            foreach (var item in _monsterAI.ItemsBeforeDeciding(Match))
            {
                if (Match.UseItem(Side.Monster, item) != ItemUseResult.Used)
                    continue;
                ApplyItemEffect(Side.Monster, item);
                if (item == ItemType.Reroll)
                    yield return PlayReroll();
            }

            var decision = _monsterAI.Decide(Match);
            foreach (var item in _monsterAI.ItemsAfterDeciding(Match, decision))
            {
                if (Match.UseItem(Side.Monster, item) == ItemUseResult.Used)
                    ApplyItemEffect(Side.Monster, item);
            }

            if (decision.IsChallenge)
            {
                _log.Add(_text.Speech(Monster.DisplayName, Monster.ChallengeLine), LogKind.MonsterSpeech);
                yield return Resolve(Match.Challenge(Side.Monster));
                yield break;
            }

            Match.PlaceBid(Side.Monster, decision.Bid);
            _log.Add(_text.Speech(Monster.DisplayName, Monster.BidLine(decision.Bid)), LogKind.MonsterSpeech);
            LogSkippedTurn();
            AdvanceTurn();
        }

        private void ApplyItemEffect(Side side, ItemType item)
        {
            if (_text.AnnouncesUse(side, item))
                _log.Add(_text.ItemUsed(side, item), side == Side.Player ? LogKind.PlayerAction : LogKind.MonsterSpeech);
            if (item == ItemType.ExtraDie)
                table.ShowDice(Match);
        }

        private IEnumerator PlayReroll()
        {
            IsBusy = true;
            Refresh();
            yield return table.PlayReroll(Match);
            IsBusy = false;
            Refresh();
        }

        private void LogSkippedTurn()
        {
            if (Match.SkippedSide.HasValue)
                _log.Add(_text.TurnSkipped(Match.SkippedSide.Value), LogKind.System);
        }

        private IEnumerator Resolve(RoundResult result)
        {
            IsBusy = true;
            Refresh();
            yield return table.PlayReveal(Match);

            var wildCounted = Match.Rules.IsWildFor(result.Bid.Face, Match.WildActive);
            _log.Add(_text.RoundResult(result, wildCounted), LogKind.Result);

            if (Match.Phase == MatchPhase.MatchOver)
            {
                var winner = Match.Winner.Value;
                _log.Add(_text.MatchOver(winner), LogKind.Result);
                hud.SetContinueLabel(_text.MatchOverLabel(winner, ExitRequested != null, EndsGame));
                IsBusy = false;
                Refresh();
                MatchFinished?.Invoke(winner);
                matchFinished.Invoke(winner);
                yield break;
            }

            hud.SetContinueLabel(LiarDiceText.NextRoundLabel);
            IsBusy = false;
            Refresh();
        }

        private void Refresh()
        {
            var state = _panelRules.Evaluate(Match, _believed, IsBusy);
            hud.ShowOxygen(Match);
            hud.SetStatus(_text.Status(Match));
            hud.ApplyPanelState(state);
            hud.ShowItems(Match, state.ItemsEnabled);
        }

        private void UnbindHud()
        {
            if (hud == null)
                return;
            hud.BelieveClicked -= Believe;
            hud.BluffClicked -= ChallengeAsPlayer;
            hud.BidSubmitted -= OnBidSubmitted;
            hud.ContinueClicked -= Continue;
            hud.ItemClicked -= OnItemClicked;
        }
    }
}
