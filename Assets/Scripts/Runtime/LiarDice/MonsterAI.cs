using System;
using System.Collections.Generic;

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
            return Decide(match.Settings.Rules, match.GetDice(Side.Monster), match.GetDice(Side.Player).Count,
                match.CurrentBid, match.WildActive);
        }

        public MonsterDecision Decide(IReadOnlyList<int> ownDice, int unknownDiceCount, Bid? currentBid,
            bool wildActive)
        {
            return Decide(_rules, ownDice, unknownDiceCount, currentBid, wildActive);
        }

        public double Probability(IReadOnlyList<int> ownDice, int unknownDiceCount, Bid bid, bool wildActive)
        {
            return Probability(_rules, ownDice, unknownDiceCount, bid, wildActive);
        }

        private MonsterDecision Decide(ILiarDiceRules rules, IReadOnlyList<int> ownDice, int unknownDiceCount,
            Bid? currentBid, bool wildActive)
        {
            var totalDice = ownDice.Count + unknownDiceCount;

            if (currentBid.HasValue)
            {
                if (!rules.CanRaise(currentBid, totalDice))
                    return MonsterDecision.Challenge();

                var p = Probability(rules, ownDice, unknownDiceCount, currentBid.Value, wildActive);
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
                    var p = Probability(rules, ownDice, unknownDiceCount, bid, wildAfterBid);
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

        private static double Probability(ILiarDiceRules rules, IReadOnlyList<int> ownDice, int unknownDiceCount,
            Bid bid, bool wildActive)
        {
            var known = rules.CountMatching(ownDice, bid.Face, wildActive);
            var needed = bid.Quantity - known;
            if (needed <= 0)
                return 1.0;
            if (needed > unknownDiceCount)
                return 0.0;

            var faceChance = rules.IsWildFor(bid.Face, wildActive) ? 2.0 / 6.0 : 1.0 / 6.0;
            return BinomialAtLeast(unknownDiceCount, needed, faceChance);
        }

        private static double BinomialAtLeast(int trials, int atLeast, double p)
        {
            var sum = 0.0;
            for (var k = atLeast; k <= trials; k++)
                sum += Combination(trials, k) * Math.Pow(p, k) * Math.Pow(1 - p, trials - k);
            return Math.Min(1.0, sum);
        }

        private static double Combination(int n, int k)
        {
            var result = 1.0;
            for (var i = 1; i <= k; i++)
                result = result * (n - k + i) / i;
            return result;
        }
    }
}
