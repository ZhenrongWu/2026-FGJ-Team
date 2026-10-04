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

    public readonly struct RoundResult
    {
        public readonly Side Challenger;
        public readonly Side Bidder;
        public readonly Bid Bid;
        public readonly int ActualCount;
        public readonly Side Loser;
        public readonly bool OxygenStolen;

        public RoundResult(Side challenger, Side bidder, Bid bid, int actualCount, Side loser,
            bool oxygenStolen = false)
        {
            Challenger = challenger;
            Bidder = bidder;
            Bid = bid;
            ActualCount = actualCount;
            Loser = loser;
            OxygenStolen = oxygenStolen;
        }

        public bool ChallengerWon => Loser == Bidder;
        public Side Winner => Loser.Opponent();
    }

    public sealed class LiarDiceMatch
    {
        private const int OxygenPerLoss = 1;
        private const int StolenOxygen = 1;

        public event Action<Side> RoundStarted;
        public event Action<Side, Bid> BidPlaced;
        public event Action<RoundResult> RoundResolved;
        public event Action<Side> MatchEnded;

        private readonly MatchSettings _settings;
        private readonly IDiceRoller _roller;
        private readonly IItemDealer _dealer;
        private readonly List<int> _playerDice;
        private readonly List<int> _monsterDice;
        private readonly List<ItemType> _playerItems = new List<ItemType>();
        private readonly List<ItemType> _monsterItems = new List<ItemType>();
        private readonly HashSet<Side> _pendingPeeks = new HashSet<Side>();
        private readonly Dictionary<Side, PeekResult> _peeks = new Dictionary<Side, PeekResult>();
        private readonly HashSet<Side> _sealedSides = new HashSet<Side>();
        private readonly HashSet<Side> _stealArmed = new HashSet<Side>();
        private readonly Dictionary<Side, int> _secretDice = new Dictionary<Side, int>
        {
            { Side.Player, 0 },
            { Side.Monster, 0 }
        };

        public MatchPhase Phase { get; private set; } = MatchPhase.NotStarted;
        public Side CurrentTurn { get; private set; }
        public Bid? CurrentBid { get; private set; }
        public Side? CurrentBidder { get; private set; }
        public Side? SkippedSide { get; private set; }
        public bool WildActive { get; private set; } = true;
        public int PlayerOxygen { get; private set; }
        public int MonsterOxygen { get; private set; }
        public RoundResult? LastResult { get; private set; }
        public Side? Winner { get; private set; }

        public MatchSettings Settings => _settings;
        public ILiarDiceRules Rules => _settings.Rules;
        public int TotalDice => _playerDice.Count + _monsterDice.Count;
        public bool CanChallenge => Phase == MatchPhase.Bidding && CurrentBid.HasValue && CurrentBidder != CurrentTurn;
        public bool MustRaise => Phase == MatchPhase.Bidding && CurrentBid.HasValue && CurrentBidder == CurrentTurn;
        public bool CanRaise => Phase == MatchPhase.Bidding && Rules.CanRaise(CurrentBid, TotalDiceKnownTo(CurrentTurn));

        public LiarDiceMatch(MatchSettings settings, IDiceRoller roller, IItemDealer dealer = null)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _roller = roller ?? throw new ArgumentNullException(nameof(roller));
            _dealer = dealer ?? new RandomItemDealer();
            _playerDice = new List<int>(new int[settings.PlayerDiceCount]);
            _monsterDice = new List<int>(new int[settings.MonsterDiceCount]);
            PlayerOxygen = settings.PlayerOxygen;
            MonsterOxygen = settings.MonsterOxygen;
        }

        public IReadOnlyList<int> GetDice(Side side) => DiceOf(side);

        public IReadOnlyList<ItemType> GetItems(Side side) => ItemsOf(side);

        public int GetOxygen(Side side) => side == Side.Player ? PlayerOxygen : MonsterOxygen;

        public int GetMaxOxygen(Side side) => side == Side.Player ? _settings.PlayerMaxOxygen : _settings.MonsterOxygen;

        public int VisibleDiceCount(Side observer, Side owner)
        {
            var count = DiceOf(owner).Count;
            return observer == owner ? count : count - _secretDice[owner];
        }

        public int TotalDiceKnownTo(Side observer)
        {
            return VisibleDiceCount(observer, Side.Player) + VisibleDiceCount(observer, Side.Monster);
        }

        public bool HasPendingPeek(Side side) => _pendingPeeks.Contains(side);

        public PeekResult? GetPeek(Side side) => _peeks.TryGetValue(side, out var peek) ? peek : (PeekResult?)null;

        public bool IsSealed(Side side) => _sealedSides.Contains(side);

        public bool IsStealArmed(Side side) => _stealArmed.Contains(side);

        public void Start()
        {
            EnsurePhase(MatchPhase.NotStarted);
            Deal(_playerItems);
            Deal(_monsterItems);
            StartRound();
        }

        public void NextRound()
        {
            EnsurePhase(MatchPhase.RoundOver);
            StartRound();
        }

        public BidValidation PlaceBid(Side side, Bid bid)
        {
            EnsurePhase(MatchPhase.Bidding);
            EnsureTurn(side);

            var validation = Rules.Validate(CurrentBid, bid, TotalDiceKnownTo(side));
            if (validation != BidValidation.Valid)
                return validation;

            CurrentBid = bid;
            CurrentBidder = side;
            if (Rules.CancelsWild(bid))
                WildActive = false;

            var opponent = side.Opponent();
            var opponentSkipped = IsSealed(opponent) && Rules.CanRaise(bid, TotalDiceKnownTo(side));
            SkippedSide = opponentSkipped ? opponent : (Side?)null;
            if (opponentSkipped)
                _sealedSides.Remove(opponent);
            CurrentTurn = opponentSkipped ? side : opponent;

            BidPlaced?.Invoke(side, bid);
            return BidValidation.Valid;
        }

        public ItemUseResult UseItem(Side side, ItemType item)
        {
            EnsurePhase(MatchPhase.Bidding);
            EnsureTurn(side);

            var items = ItemsOf(side);
            if (!items.Contains(item))
                return ItemUseResult.NotHeld;
            if (IsAlreadyActive(side, item))
                return ItemUseResult.AlreadyActive;

            items.Remove(item);
            SkippedSide = null;
            switch (item)
            {
                case ItemType.PeekLens:
                    _pendingPeeks.Add(side);
                    break;
                case ItemType.ExtraDie:
                    DiceOf(side).Add(_roller.Roll());
                    _secretDice[side]++;
                    break;
                case ItemType.Reroll:
                    RollAll();
                    RefreshPeeks();
                    break;
                case ItemType.SealTape:
                    _sealedSides.Add(side.Opponent());
                    break;
                case ItemType.StealOxygen:
                    _stealArmed.Add(side);
                    break;
            }
            return ItemUseResult.Used;
        }

        public RoundResult Challenge(Side challenger)
        {
            EnsurePhase(MatchPhase.Bidding);
            EnsureTurn(challenger);
            if (!CurrentBid.HasValue)
                throw new InvalidOperationException("尚未有人喊數，無法質疑。");
            if (CurrentBidder == challenger)
                throw new InvalidOperationException("不能質疑自己的喊數。");

            var bid = CurrentBid.Value;
            var bidder = CurrentBidder.Value;
            var actual = Rules.CountMatching(_playerDice.Concat(_monsterDice), bid.Face, WildActive);
            var loser = actual >= bid.Quantity ? challenger : bidder;
            var winner = loser.Opponent();
            var stolen = IsStealArmed(winner);

            ChangeOxygen(loser, -(OxygenPerLoss + (stolen ? StolenOxygen : 0)));
            if (stolen)
                ChangeOxygen(winner, StolenOxygen);

            _stealArmed.Clear();
            _secretDice[Side.Player] = 0;
            _secretDice[Side.Monster] = 0;
            SkippedSide = null;

            var result = new RoundResult(challenger, bidder, bid, actual, loser, stolen);
            LastResult = result;
            Phase = GetOxygen(loser) <= 0 ? MatchPhase.MatchOver : MatchPhase.RoundOver;

            RoundResolved?.Invoke(result);
            if (Phase == MatchPhase.MatchOver)
            {
                Winner = winner;
                MatchEnded?.Invoke(winner);
            }
            return result;
        }

        private void StartRound()
        {
            RollAll();
            CurrentBid = null;
            CurrentBidder = null;
            WildActive = Rules.OnesAreWild;
            _peeks.Clear();
            foreach (var side in new[] { Side.Player, Side.Monster })
            {
                if (_pendingPeeks.Remove(side))
                    _peeks[side] = Peek(side, _roller.Roll());
            }

            var starter = _settings.FirstTurn;
            SkippedSide = IsSealed(starter) ? starter : (Side?)null;
            if (SkippedSide.HasValue)
            {
                _sealedSides.Remove(starter);
                starter = starter.Opponent();
            }

            CurrentTurn = starter;
            Phase = MatchPhase.Bidding;
            RoundStarted?.Invoke(starter);
        }

        private bool IsAlreadyActive(Side side, ItemType item)
        {
            switch (item)
            {
                case ItemType.PeekLens:
                    return HasPendingPeek(side);
                case ItemType.SealTape:
                    return IsSealed(side.Opponent());
                case ItemType.StealOxygen:
                    return IsStealArmed(side);
                default:
                    return false;
            }
        }

        private PeekResult Peek(Side observer, int face)
        {
            return new PeekResult(face, DiceOf(observer.Opponent()).Count(die => die == face));
        }

        private void RefreshPeeks()
        {
            foreach (var side in _peeks.Keys.ToList())
                _peeks[side] = Peek(side, _peeks[side].Face);
        }

        private void ChangeOxygen(Side side, int amount)
        {
            var value = Math.Max(0, Math.Min(GetMaxOxygen(side), GetOxygen(side) + amount));
            if (side == Side.Player)
                PlayerOxygen = value;
            else
                MonsterOxygen = value;
        }

        private void Deal(List<ItemType> items)
        {
            for (var i = 0; i < _settings.ItemCount; i++)
                items.Add(_dealer.Draw());
        }

        private void RollAll()
        {
            Roll(_playerDice);
            Roll(_monsterDice);
        }

        private void Roll(List<int> dice)
        {
            for (var i = 0; i < dice.Count; i++)
                dice[i] = _roller.Roll();
        }

        private List<int> DiceOf(Side side) => side == Side.Player ? _playerDice : _monsterDice;

        private List<ItemType> ItemsOf(Side side) => side == Side.Player ? _playerItems : _monsterItems;

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
