using System;
using System.Collections.Generic;

namespace FGJ.LiarDice
{
    /// <summary>怪物個性參數，由 <see cref="LiarDiceConfig"/> 產生。</summary>
    public sealed class MonsterAIProfile
    {
        /// <summary>上一個喊數成立機率低於此值就質疑。</summary>
        public float ChallengeThreshold { get; }

        /// <summary>成立機率不低於此值的喊數視為「有把握」，會從中挑最大的喊。</summary>
        public float ConfidentBidThreshold { get; }

        /// <summary>刻意吹牛（喊沒把握的數）的機率。</summary>
        public float BluffChance { get; }

        /// <summary>一次加注最多比目前數量多幾個。</summary>
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

    /// <summary>依自己的骰子與未知骰子的二項分布機率決定要加注還是質疑。</summary>
    public sealed class MonsterAI
    {
        private readonly MonsterAIProfile _profile;
        private readonly Random _random;

        public MonsterAI(MonsterAIProfile profile, Random random = null)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _random = random ?? new Random();
        }

        public MonsterDecision Decide(LiarDiceMatch match)
        {
            return Decide(match.GetDice(Side.Monster), match.GetDice(Side.Player).Count, match.CurrentBid,
                match.WildActive);
        }

        public MonsterDecision Decide(IReadOnlyList<int> ownDice, int unknownDiceCount, Bid? currentBid,
            bool wildActive)
        {
            var totalDice = ownDice.Count + unknownDiceCount;

            if (currentBid.HasValue)
            {
                if (!LiarDiceRules.CanRaise(currentBid, totalDice))
                    return MonsterDecision.Challenge();

                var p = Probability(ownDice, unknownDiceCount, currentBid.Value, wildActive);
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
                    if (LiarDiceRules.Validate(currentBid, bid, totalDice) != BidValidation.Valid)
                        continue;

                    var wildAfterBid = wildActive && !LiarDiceRules.CancelsWild(bid);
                    var p = Probability(ownDice, unknownDiceCount, bid, wildAfterBid);
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

            // 有把握的喊數中挑最大的施壓；都沒把握就喊成立機率最高的。
            if (confident.Count > 0)
                return MonsterDecision.Raise(confident[confident.Count - 1]);
            return MonsterDecision.Raise(bestBid);
        }

        /// <summary>在已知自己骰子的情況下，喊數成立（全場符合數量 ≥ Quantity）的機率。</summary>
        public static double Probability(IReadOnlyList<int> ownDice, int unknownDiceCount, Bid bid, bool wildActive)
        {
            var known = LiarDiceRules.CountMatching(ownDice, bid.Face, wildActive);
            var needed = bid.Quantity - known;
            if (needed <= 0)
                return 1.0;
            if (needed > unknownDiceCount)
                return 0.0;

            var faceChance = wildActive && bid.Face != LiarDiceRules.WildFace ? 2.0 / 6.0 : 1.0 / 6.0;
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
