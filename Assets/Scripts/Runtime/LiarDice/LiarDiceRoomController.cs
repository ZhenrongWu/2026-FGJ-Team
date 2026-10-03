using System;
using System.Collections;
using FGJ.LiarDice.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace FGJ.LiarDice
{
    public sealed class LiarDiceRoomController : MonoBehaviour
    {
        [SerializeField] private LiarDiceConfig config;
        [SerializeField] private LiarDiceView view;
        [Min(0f)] [SerializeField] private float monsterThinkSeconds = 1.2f;
        [SerializeField] private UnityEvent<Side> matchFinished = new UnityEvent<Side>();

        private MonsterAI _monster;
        private Func<IDiceRoller> _rollerFactory;
        private Coroutine _monsterTurn;

        public event Action<Side> MatchFinished;
        public event Action<Side> ExitRequested;

        public LiarDiceMatch Match { get; private set; }
        public LiarDiceView View => view;
        public bool IsPlayerTurn => Match != null && Match.Phase == MatchPhase.Bidding && Match.CurrentTurn == Side.Player;

        public void Configure(LiarDiceConfig roomConfig, LiarDiceView roomView)
        {
            config = roomConfig;
            view = roomView;
        }

        public void UseConfig(LiarDiceConfig roomConfig)
        {
            config = roomConfig;
        }

        public void SetMonsterThinkSeconds(float seconds)
        {
            monsterThinkSeconds = Mathf.Max(0f, seconds);
        }

        private void Start()
        {
            if (Match == null && config != null)
                Begin(config.ToMatchSettings(), () => new RandomDiceRoller(),
                    new MonsterAI(config.ToMonsterProfile(), rules: config.ToRules()));
        }

        private void OnDestroy()
        {
            UnbindView();
        }

        public void Begin(MatchSettings settings, Func<IDiceRoller> rollerFactory, MonsterAI monster)
        {
            _rollerFactory = rollerFactory ?? throw new ArgumentNullException(nameof(rollerFactory));
            _monster = monster ?? throw new ArgumentNullException(nameof(monster));
            StopMonsterTurn();

            UnbindView();
            view.BidSubmitted += OnBidSubmitted;
            view.ChallengeRequested += ChallengeAsPlayer;
            view.ContinueRequested += Continue;

            Match = new LiarDiceMatch(settings, _rollerFactory());
            Match.Start();
            view.HideResult();
            view.ClearInput();
            view.ClearError();
            view.SetMonsterLine(LiarDiceText.MonsterGreeting);
            AdvanceTurn();
        }

        public BidInputResult SubmitPlayerBid(string quantityText, string faceText)
        {
            if (!IsPlayerTurn)
            {
                view.ShowError(LiarDiceText.NotPlayerTurn);
                return BidInputResult.Fail(LiarDiceText.NotPlayerTurn);
            }

            var input = new BidInputValidator(Match.Rules).Validate(quantityText, faceText, Match.CurrentBid,
                Match.TotalDice);
            if (!input.IsValid)
            {
                view.ShowError(input.ErrorMessage);
                return input;
            }

            Match.PlaceBid(Side.Player, input.Bid);
            view.ClearError();
            view.ClearInput();
            AdvanceTurn();
            return input;
        }

        public void ChallengeAsPlayer()
        {
            if (!IsPlayerTurn || !Match.CanChallenge)
            {
                view.ShowError(IsPlayerTurn ? LiarDiceText.NothingToChallenge : LiarDiceText.NotPlayerTurn);
                return;
            }

            view.ClearError();
            Resolve(Match.Challenge(Side.Player));
        }

        public void Continue()
        {
            if (Match == null)
                return;

            switch (Match.Phase)
            {
                case MatchPhase.RoundOver:
                    Match.NextRound();
                    view.HideResult();
                    AdvanceTurn();
                    break;
                case MatchPhase.MatchOver:
                    if (ExitRequested != null)
                        ExitRequested.Invoke(Match.Winner.Value);
                    else
                        Begin(Match.Settings, _rollerFactory, _monster);
                    break;
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !IsPlayerTurn)
                return;

            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
                view.SubmitBid();
            else if (keyboard.tabKey.wasPressedThisFrame)
                view.CycleInputFocus();
        }

        private void OnBidSubmitted(string quantityText, string faceText)
        {
            SubmitPlayerBid(quantityText, faceText);
        }

        private void AdvanceTurn()
        {
            view.Render(Match, false);
            if (Match.Phase != MatchPhase.Bidding)
                return;

            if (Match.CurrentTurn == Side.Monster)
                _monsterTurn = StartCoroutine(PlayMonsterTurn());
            else
                view.FocusQuantity();
        }

        private IEnumerator PlayMonsterTurn()
        {
            if (monsterThinkSeconds > 0f)
                yield return new WaitForSeconds(monsterThinkSeconds);
            else
                yield return null;

            _monsterTurn = null;
            var decision = _monster.Decide(Match);
            if (decision.IsChallenge)
            {
                view.SetMonsterLine(LiarDiceText.MonsterChallengeLine);
                Resolve(Match.Challenge(Side.Monster));
                yield break;
            }

            Match.PlaceBid(Side.Monster, decision.Bid);
            view.SetMonsterLine(LiarDiceText.MonsterBidLine(decision.Bid));
            AdvanceTurn();
        }

        private void Resolve(RoundResult result)
        {
            view.Render(Match, true);

            var message = LiarDiceText.RoundResult(result, Match.Rules.IsWildFor(result.Bid.Face, Match.WildActive));
            if (Match.Phase == MatchPhase.MatchOver)
            {
                var winner = Match.Winner.Value;
                view.ShowResult($"{message}\n\n{LiarDiceText.MatchOver(winner)}", LiarDiceText.MatchOverLabel(winner,
                    ExitRequested != null));
                MatchFinished?.Invoke(winner);
                matchFinished.Invoke(winner);
                return;
            }

            view.ShowResult(message, LiarDiceText.NextRoundLabel);
        }

        private void StopMonsterTurn()
        {
            if (_monsterTurn == null)
                return;
            StopCoroutine(_monsterTurn);
            _monsterTurn = null;
        }

        private void UnbindView()
        {
            if (view == null)
                return;
            view.BidSubmitted -= OnBidSubmitted;
            view.ChallengeRequested -= ChallengeAsPlayer;
            view.ContinueRequested -= Continue;
        }
    }
}
