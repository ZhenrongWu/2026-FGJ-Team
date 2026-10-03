using System;
using FGJ.LiarDice;
using NUnit.Framework;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class MonsterAITests
    {
        private static MonsterAI CreateAI(float bluffChance = 0f)
        {
            return new MonsterAI(new MonsterAIProfile(0.35f, 0.5f, bluffChance, 2), new Random(42));
        }

        [Test]
        public void Probability_KnownDiceAlreadyEnough_IsOne()
        {
            Assert.AreEqual(1.0, CreateAI().Probability(new[] { 4, 4, 1, 2, 3 }, 5, new Bid(3, 4), true));
        }

        [Test]
        public void Probability_NeedMoreThanUnknownDice_IsZero()
        {
            Assert.AreEqual(0.0, CreateAI().Probability(new[] { 2, 2, 2, 2, 2 }, 5, new Bid(6, 4), true));
        }

        [Test]
        public void Probability_MatchesBinomial()
        {
            Assert.AreEqual(1.0 / 3.0, CreateAI().Probability(new[] { 2, 3, 5, 6, 6 }, 1, new Bid(1, 4), true), 1e-9);
            Assert.AreEqual(1.0 / 6.0, CreateAI().Probability(new[] { 2, 3, 5, 6, 6 }, 1, new Bid(1, 4), false), 1e-9);
        }

        [Test]
        public void Decide_ImpossibleBid_Challenges()
        {
            var decision = CreateAI().Decide(new[] { 2, 2, 2, 2, 2 }, 5, new Bid(7, 4), true);
            Assert.IsTrue(decision.IsChallenge);
        }

        [Test]
        public void Decide_CertainBid_RaisesWithLegalBid()
        {
            var current = new Bid(3, 4);
            var decision = CreateAI().Decide(new[] { 4, 4, 4, 1, 1 }, 5, current, true);

            Assert.IsFalse(decision.IsChallenge);
            Assert.AreEqual(BidValidation.Valid, new LiarDiceRules().Validate(current, decision.Bid, 10));
        }

        [Test]
        public void Decide_MaximumBid_MustChallenge()
        {
            var decision = CreateAI().Decide(new[] { 6, 6, 6, 6, 6 }, 5, new Bid(10, 6), true);
            Assert.IsTrue(decision.IsChallenge);
        }

        [Test]
        public void Decide_OpeningBid_IsLegalAndConfident()
        {
            var ownDice = new[] { 5, 5, 5, 2, 3 };
            var decision = CreateAI().Decide(ownDice, 5, null, true);

            Assert.IsFalse(decision.IsChallenge);
            Assert.AreEqual(BidValidation.Valid, new LiarDiceRules().Validate(null, decision.Bid, 10));
            Assert.GreaterOrEqual(CreateAI().Probability(ownDice, 5, decision.Bid, decision.Bid.Face != 1), 0.5);
        }

        [Test]
        public void Decide_AlwaysBluff_StillProducesLegalBids()
        {
            var ai = CreateAI(1f);
            var current = new Bid(2, 3);
            for (var i = 0; i < 20; i++)
            {
                var decision = ai.Decide(new[] { 3, 3, 1, 2, 6 }, 5, current, true);
                Assert.IsFalse(decision.IsChallenge);
                Assert.AreEqual(BidValidation.Valid, new LiarDiceRules().Validate(current, decision.Bid, 10));
            }
        }

        [Test]
        public void Probability_OnesNotWild_UsesSingleFaceChance()
        {
            var ai = new MonsterAI(new MonsterAIProfile(), new Random(1), new LiarDiceRules(onesAreWild: false));
            Assert.AreEqual(1.0 / 6.0, ai.Probability(new[] { 2, 3, 5, 6, 6 }, 1, new Bid(1, 4), true), 1e-9);
        }
    }
}
