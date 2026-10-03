using System;
using System.Collections.Generic;
using System.Linq;

namespace FGJ.LiarDice
{
    public enum MatchPhase
    {
        NotStarted,
        Bidding,
        RoundOver,
        MatchOver
    }

    /// <summary>一次質疑的結算結果。</summary>
    public readonly struct RoundResult
    {
        public readonly Side Challenger;
        public readonly Side Bidder;
        public readonly Bid Bid;
        public readonly int ActualCount;
        public readonly Side Loser;

        public RoundResult(Side challenger, Side bidder, Bid bid, int actualCount, Side loser)
        {
            Challenger = challenger;
            Bidder = bidder;
            Bid = bid;
            ActualCount = actualCount;
            Loser = loser;
        }

        public bool ChallengerWon => Loser == Bidder;
    }

    /// <summary>
    /// 吹牛對局狀態機：搖骰 → 輪流喊數 → 質疑開盅 → 輸家扣氧氣 → 輸家先叫下一局，直到一方氧氣歸零。
    /// </summary>
    public sealed class LiarDiceMatch
    {
        public event Action<Side> RoundStarted;
        public event Action<Side, Bid> BidPlaced;
        public event Action<RoundResult> RoundResolved;
        public event Action<Side> MatchEnded;

        private readonly MatchSettings _settings;
        private readonly IDiceRoller _roller;
        private readonly int[] _playerDice;
        private readonly int[] _monsterDice;

        public MatchPhase Phase { get; private set; } = MatchPhase.NotStarted;
        public Side CurrentTurn { get; private set; }
        public Bid? CurrentBid { get; private set; }
        public Side? CurrentBidder { get; private set; }
        public bool WildActive { get; private set; } = true;
        public int PlayerOxygen { get; private set; }
        public int MonsterOxygen { get; private set; }
        public RoundResult? LastResult { get; private set; }
        public Side? Winner { get; private set; }

        public int TotalDice => _playerDice.Length + _monsterDice.Length;
        public bool CanChallenge => Phase == MatchPhase.Bidding && CurrentBid.HasValue;
        public bool CanRaise => Phase == MatchPhase.Bidding && LiarDiceRules.CanRaise(CurrentBid, TotalDice);

        public LiarDiceMatch(MatchSettings settings, IDiceRoller roller)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _roller = roller ?? throw new ArgumentNullException(nameof(roller));
            _playerDice = new int[settings.PlayerDiceCount];
            _monsterDice = new int[settings.MonsterDiceCount];
            PlayerOxygen = settings.PlayerOxygen;
            MonsterOxygen = settings.MonsterOxygen;
        }

        public IReadOnlyList<int> GetDice(Side side) => side == Side.Player ? _playerDice : _monsterDice;

        public int GetOxygen(Side side) => side == Side.Player ? PlayerOxygen : MonsterOxygen;

        public void Start()
        {
            EnsurePhase(MatchPhase.NotStarted);
            StartRound(_settings.FirstTurn);
        }

        /// <summary>開始下一局，由上一局的輸家先叫。</summary>
        public void NextRound()
        {
            EnsurePhase(MatchPhase.RoundOver);
            StartRound(LastResult.Value.Loser);
        }

        /// <summary>喊數。不合法時回傳原因且狀態不變。</summary>
        public BidValidation PlaceBid(Side side, Bid bid)
        {
            EnsurePhase(MatchPhase.Bidding);
            EnsureTurn(side);

            var validation = LiarDiceRules.Validate(CurrentBid, bid, TotalDice);
            if (validation != BidValidation.Valid)
                return validation;

            CurrentBid = bid;
            CurrentBidder = side;
            if (LiarDiceRules.CancelsWild(bid))
                WildActive = false;
            CurrentTurn = LiarDiceRules.Opponent(side);

            BidPlaced?.Invoke(side, bid);
            return BidValidation.Valid;
        }

        /// <summary>質疑上一個喊數並開盅結算。</summary>
        public RoundResult Challenge(Side challenger)
        {
            EnsurePhase(MatchPhase.Bidding);
            EnsureTurn(challenger);
            if (!CurrentBid.HasValue)
                throw new InvalidOperationException("尚未有人喊數，無法質疑。");

            var bid = CurrentBid.Value;
            var bidder = CurrentBidder.Value;
            var actual = LiarDiceRules.CountMatching(_playerDice.Concat(_monsterDice), bid.Face, WildActive);
            var loser = actual >= bid.Quantity ? challenger : bidder;

            if (loser == Side.Player)
                PlayerOxygen--;
            else
                MonsterOxygen--;

            var result = new RoundResult(challenger, bidder, bid, actual, loser);
            LastResult = result;
            Phase = GetOxygen(loser) <= 0 ? MatchPhase.MatchOver : MatchPhase.RoundOver;

            RoundResolved?.Invoke(result);
            if (Phase == MatchPhase.MatchOver)
            {
                Winner = LiarDiceRules.Opponent(loser);
                MatchEnded?.Invoke(Winner.Value);
            }
            return result;
        }

        private void StartRound(Side starter)
        {
            Roll(_playerDice);
            Roll(_monsterDice);
            CurrentBid = null;
            CurrentBidder = null;
            WildActive = true;
            CurrentTurn = starter;
            Phase = MatchPhase.Bidding;
            RoundStarted?.Invoke(starter);
        }

        private void Roll(int[] dice)
        {
            for (var i = 0; i < dice.Length; i++)
                dice[i] = _roller.Roll();
        }

        private void EnsurePhase(MatchPhase expected)
        {
            if (Phase != expected)
                throw new InvalidOperationException($"目前階段為 {Phase}，需要 {expected}。");
        }

        private void EnsureTurn(Side side)
        {
            if (side != CurrentTurn)
                throw new InvalidOperationException($"現在輪到 {CurrentTurn}，不是 {side}。");
        }
    }
}
