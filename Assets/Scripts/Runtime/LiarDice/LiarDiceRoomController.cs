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
        [Min(0f)] [SerializeField] private float monsterThinkSeconds = 1.2f;
        [SerializeField] private UnityEvent<Side> matchFinished = new UnityEvent<Side>();

        private readonly MatchLog _log = new MatchLog();
        private readonly LiarDiceText _text = new LiarDiceText();
        private readonly BidPanelRules _panelRules = new BidPanelRules();
        private MonsterAI _monsterAI;
        private Func<IDiceRoller> _rollerFactory;
        private MonsterProfile _fallbackMonster;
        private bool _believed;
        private int _roundNumber;

        public event Action<Side> MatchFinished;
        public event Action<Side> ExitRequested;

        public LiarDiceMatch Match { get; private set; }
        public LiarDiceHud Hud => hud;
        public DiceTableView Table => table;
        public MatchLog Log => _log;
        public bool IsBusy { get; private set; }
        public bool HasBelieved => _believed;
        public bool IsPlayerTurn => Match != null && !IsBusy && Match.Phase == MatchPhase.Bidding &&
                                    Match.CurrentTurn == Side.Player;
        public MonsterProfile Monster => monster != null ? monster : FallbackMonster;

        private MonsterProfile FallbackMonster => _fallbackMonster != null
            ? _fallbackMonster
            : _fallbackMonster = ScriptableObject.CreateInstance<MonsterProfile>();

        public void Configure(LiarDiceHud roomHud, DiceTableView diceTable)
        {
            hud = roomHud;
            table = diceTable;
        }

        public void UseEncounter(LiarDiceConfig roomConfig, MonsterProfile roomMonster)
        {
            if (roomConfig != null)
                config = roomConfig;
            if (roomMonster != null)
                monster = roomMonster;
        }

        public void SetMonsterThinkSeconds(float seconds)
        {
            monsterThinkSeconds = Mathf.Max(0f, seconds);
        }

        private void Start()
        {
            if (Match == null && config != null)
                Begin(config.ToMatchSettings(), () => new RandomDiceRoller(),
                    new MonsterAI(Monster.ToAIProfile(), rules: config.ToRules()));
        }

        private void OnDestroy()
        {
            if (_fallbackMonster != null)
                Destroy(_fallbackMonster);
            UnbindHud();
        }

        public void Begin(MatchSettings settings, Func<IDiceRoller> rollerFactory, MonsterAI monsterAI)
        {
            _rollerFactory = rollerFactory ?? throw new ArgumentNullException(nameof(rollerFactory));
            _monsterAI = monsterAI ?? throw new ArgumentNullException(nameof(monsterAI));
            StopAllCoroutines();
            IsBusy = false;

            UnbindHud();
            hud.BelieveClicked += Believe;
            hud.BluffClicked += ChallengeAsPlayer;
            hud.BidSubmitted += OnBidSubmitted;
            hud.ContinueClicked += Continue;

            Match = new LiarDiceMatch(settings, _rollerFactory());
            Match.Start();
            _roundNumber = 0;
            _log.Clear();
            hud.LogView.Bind(_log);
            hud.ClearError();
            hud.ClearBidInput();
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
                Match.TotalDice);
            if (!input.IsValid)
            {
                hud.ShowError(input.ErrorMessage);
                return input;
            }

            var answeringBid = Match.CurrentBid.HasValue;
            Match.PlaceBid(Side.Player, input.Bid);
            _log.Add(_text.PlayerBid(input.Bid, answeringBid), LogKind.PlayerAction);
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
                        Begin(Match.Settings, _rollerFactory, _monsterAI);
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

        private void StartRound()
        {
            _roundNumber++;
            _believed = false;
            _log.Add(_text.RoundStart(_roundNumber, Match.CurrentTurn), LogKind.System);
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
            else if (!Match.CurrentBid.HasValue)
                hud.FocusQuantity();
        }

        private IEnumerator PlayMonsterTurn()
        {
            if (monsterThinkSeconds > 0f)
                yield return new WaitForSeconds(monsterThinkSeconds);
            else
                yield return null;

            var decision = _monsterAI.Decide(Match);
            if (decision.IsChallenge)
            {
                _log.Add(_text.Speech(Monster.DisplayName, Monster.ChallengeLine), LogKind.MonsterSpeech);
                yield return Resolve(Match.Challenge(Side.Monster));
                yield break;
            }

            Match.PlaceBid(Side.Monster, decision.Bid);
            _log.Add(_text.Speech(Monster.DisplayName, Monster.BidLine(decision.Bid)), LogKind.MonsterSpeech);
            AdvanceTurn();
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
                hud.SetContinueLabel(_text.MatchOverLabel(winner, ExitRequested != null));
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
            hud.ShowOxygen(Match);
            hud.SetStatus(_text.Status(Match));
            hud.ApplyPanelState(_panelRules.Evaluate(Match, _believed, IsBusy));
        }

        private void UnbindHud()
        {
            if (hud == null)
                return;
            hud.BelieveClicked -= Believe;
            hud.BluffClicked -= ChallengeAsPlayer;
            hud.BidSubmitted -= OnBidSubmitted;
            hud.ContinueClicked -= Continue;
        }
    }
}
