using System;
using System.Collections.Generic;
using System.Linq;

namespace FGJ.LiarDice
{
    public sealed class MonsterAIProfile
    {
        public float ChallengeThreshold { get; }

        public float ConfidentBidThreshold { get; }

        public float BluffChance { get; }

        public int MaxRaiseStep { get; }

        public MonsterAIProfile(float challengeThreshold = 0.35f, float confidentBidThreshold = 0.5f,
            float bluffChance = 0.15f, int maxRaiseStep = 2)
        {
            ChallengeThreshold = challengeThreshold;
            ConfidentBidThreshold = confidentBidThreshold;
            BluffChance = bluffChance;
            MaxRaiseStep = Math.Max(1, maxRaiseStep);
        }
    }

    public readonly struct MonsterDecision
    {
        public readonly bool IsChallenge;
        public readonly Bid Bid;

        private MonsterDecision(bool isChallenge, Bid bid)
        {
            IsChallenge = isChallenge;
            Bid = bid;
        }

        public static MonsterDecision Challenge() => new MonsterDecision(true, default);
        public static MonsterDecision Raise(Bid bid) => new MonsterDecision(false, bid);
    }

    public sealed class MonsterAI
    {
        private const double SureChallengeRatio = 0.5;
        private const double SureBidProbability = 0.85;
        private const double SealBidProbability = 0.6;
        private const int FacesPerDie = LiarDiceRules.MaxFace - LiarDiceRules.MinFace + 1;

        private readonly MonsterAIProfile _profile;
        private readonly Random _random;
        private readonly ILiarDiceRules _rules;

        public MonsterAI(MonsterAIProfile profile, Random random = null, ILiarDiceRules rules = null)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _random = random ?? new Random();
            _rules = rules ?? new LiarDiceRules();
        }

        public MonsterDecision Decide(LiarDiceMatch match)
        {
            return Decide(match.Rules, match.GetDice(Side.Monster), UnknownDice(match), match.CurrentBid,
                match.WildActive, match.GetPeek(Side.Monster), match.MustRaise);
        }

        public MonsterDecision Decide(IReadOnlyList<int> ownDice, int unknownDiceCount, Bid? currentBid,
            bool wildActive, PeekResult? peek = null, bool mustRaise = false)
        {
            return Decide(_rules, ownDice, unknownDiceCount, currentBid, wildActive, peek, mustRaise);
        }

        public double Probability(IReadOnlyList<int> ownDice, int unknownDiceCount, Bid bid, bool wildActive,
            PeekResult? peek = null)
        {
            return Probability(_rules, ownDice, unknownDiceCount, bid, wildActive, peek);
        }

        public IReadOnlyList<ItemType> ItemsBeforeDeciding(LiarDiceMatch match)
        {
            var held = match.GetItems(Side.Monster);
            var chosen = new List<ItemType>();
            if (held.Contains(ItemType.ExtraDie))
                chosen.Add(ItemType.ExtraDie);
            if (held.Contains(ItemType.PeekLens) && !match.HasPendingPeek(Side.Monster))
                chosen.Add(ItemType.PeekLens);
            if (held.Contains(ItemType.Reroll) && ShouldReroll(match))
                chosen.Add(ItemType.Reroll);
            return chosen;
        }

        public IReadOnlyList<ItemType> ItemsAfterDeciding(LiarDiceMatch match, MonsterDecision decision)
        {
            var held = match.GetItems(Side.Monster);
            var chosen = new List<ItemType>();
            if (held.Contains(ItemType.StealOxygen) && !match.IsStealArmed(Side.Monster) &&
                IsSureOfOutcome(match, decision))
                chosen.Add(ItemType.StealOxygen);
            if (held.Contains(ItemType.SealTape) && !match.IsSealed(Side.Player) && !decision.IsChallenge &&
                BidProbability(match, decision.Bid) >= SealBidProbability &&
                match.Rules.CanRaise(decision.Bid, match.TotalDiceKnownTo(Side.Monster)))
                chosen.Add(ItemType.SealTape);
            return chosen;
        }

        private bool ShouldReroll(LiarDiceMatch match)
        {
            if (!match.CurrentBid.HasValue || match.MustRaise)
                return false;
            var bid = match.CurrentBid.Value;
            var ownSupport = match.Rules.CountMatching(match.GetDice(Side.Monster), bid.Face, match.WildActive);
            return ownSupport == 0 && CurrentBidProbability(match) >= _profile.ChallengeThreshold;
        }

        private bool IsSureOfOutcome(LiarDiceMatch match, MonsterDecision decision)
        {
            if (decision.IsChallenge)
                return CurrentBidProbability(match) <= _profile.ChallengeThreshold * SureChallengeRatio;
            return BidProbability(match, decision.Bid) >= SureBidProbability;
        }

        private double CurrentBidProbability(LiarDiceMatch match)
        {
            return Probability(match.Rules, match.GetDice(Side.Monster), UnknownDice(match), match.CurrentBid.Value,
                match.WildActive, match.GetPeek(Side.Monster));
        }

        private double BidProbability(LiarDiceMatch match, Bid bid)
        {
            var wildAfterBid = match.WildActive && !match.Rules.CancelsWild(bid);
            return Probability(match.Rules, match.GetDice(Side.Monster), UnknownDice(match), bid, wildAfterBid,
                match.GetPeek(Side.Monster));
        }

        private int UnknownDice(LiarDiceMatch match) => match.VisibleDiceCount(Side.Monster, Side.Player);

        private MonsterDecision Decide(ILiarDiceRules rules, IReadOnlyList<int> ownDice, int unknownDiceCount,
            Bid? currentBid, bool wildActive, PeekResult? peek, bool mustRaise)
        {
            var totalDice = ownDice.Count + unknownDiceCount;

            if (currentBid.HasValue && !mustRaise)
            {
                if (!rules.CanRaise(currentBid, totalDice))
                    return MonsterDecision.Challenge();

                var p = Probability(rules, ownDice, unknownDiceCount, currentBid.Value, wildActive, peek);
                if (p < _profile.ChallengeThreshold)
                    return MonsterDecision.Challenge();
            }

            var confident = new List<Bid>();
            var bluffs = new List<Bid>();
            var bestBid = default(Bid);
            var bestProbability = -1.0;

            var minQuantity = currentBid?.Quantity ?? 1;
            var maxQuantity = Math.Min(totalDice, minQuantity + _profile.MaxRaiseStep);
            for (var quantity = minQuantity; quantity <= maxQuantity; quantity++)
            {
                for (var face = LiarDiceRules.MinFace; face <= LiarDiceRules.MaxFace; face++)
                {
                    var bid = new Bid(quantity, face);
                    if (rules.Validate(currentBid, bid, totalDice) != BidValidation.Valid)
                        continue;

                    var wildAfterBid = wildActive && !rules.CancelsWild(bid);
                    var p = Probability(rules, ownDice, unknownDiceCount, bid, wildAfterBid, peek);
                    if (p >= _profile.ConfidentBidThreshold)
                        confident.Add(bid);
                    else
                        bluffs.Add(bid);

                    if (p > bestProbability)
                    {
                        bestProbability = p;
                        bestBid = bid;
                    }
                }
            }

            if (bluffs.Count > 0 && _random.NextDouble() < _profile.BluffChance)
                return MonsterDecision.Raise(bluffs[_random.Next(bluffs.Count)]);

            if (confident.Count > 0)
                return MonsterDecision.Raise(confident[confident.Count - 1]);
            return MonsterDecision.Raise(bestBid);
        }

        private double Probability(ILiarDiceRules rules, IReadOnlyList<int> ownDice, int unknownDiceCount, Bid bid,
            bool wildActive, PeekResult? peek)
        {
            var matchingFaces = Enumerable.Range(LiarDiceRules.MinFace, FacesPerDie)
                .Where(face => face == bid.Face || (rules.IsWildFor(bid.Face, wildActive) && face == LiarDiceRules.WildFace))
                .ToList();

            var known = rules.CountMatching(ownDice, bid.Face, wildActive);
            var unknown = unknownDiceCount;
            var facesLeft = FacesPerDie;
            if (peek.HasValue)
            {
                var seen = Math.Min(peek.Value.Count, unknownDiceCount);
                if (matchingFaces.Remove(peek.Value.Face))
                    known += seen;
                unknown -= seen;
                facesLeft--;
            }

            var needed = bid.Quantity - known;
            if (needed <= 0)
                return 1.0;
            if (needed > unknown)
                return 0.0;

            return BinomialAtLeast(unknown, needed, (double)matchingFaces.Count / facesLeft);
        }

        private double BinomialAtLeast(int trials, int atLeast, double p)
        {
            var sum = 0.0;
            for (var k = atLeast; k <= trials; k++)
                sum += Combination(trials, k) * Math.Pow(p, k) * Math.Pow(1 - p, trials - k);
            return Math.Min(1.0, sum);
        }

        private double Combination(int n, int k)
        {
            var result = 1.0;
            for (var i = 1; i <= k; i++)
                result = result * (n - k + i) / i;
            return result;
        }
    }
}
