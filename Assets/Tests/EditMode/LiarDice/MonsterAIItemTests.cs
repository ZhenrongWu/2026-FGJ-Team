using System;
using System.Linq;
using FGJ.LiarDice;
using NUnit.Framework;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class MonsterAIItemTests
    {
        private static readonly int[] WeakMonsterRolls = { 2, 2, 2, 2, 2, 3, 3, 5, 5, 6 };
        private static readonly int[] StrongMonsterRolls = { 2, 3, 5, 6, 6, 4, 4, 4, 1, 1 };

        private static MonsterAI CreateAI()
        {
            return new MonsterAI(new MonsterAIProfile(0.35f, 0.5f, 0f, 2), new Random(42));
        }

        private static LiarDiceMatch CreateMatch(int[] rolls, params ItemType[] monsterItems)
        {
            var dealt = Enumerable.Repeat(ItemType.Reroll, monsterItems.Length).Concat(monsterItems).ToArray();
            var settings = new MatchSettings(5, 5, 3, 3, Side.Monster, null, monsterItems.Length);
            var extra = Enumerable.Repeat(4, 20);
            var match = new LiarDiceMatch(settings, new FixedDiceRoller(rolls.Concat(extra).ToArray()),
                new FixedItemDealer(dealt));
            match.Start();
            return match;
        }

        [Test]
        public void ItemsBeforeDeciding_UsesExtraDieAndPeekLensRightAway()
        {
            var match = CreateMatch(WeakMonsterRolls, ItemType.ExtraDie, ItemType.PeekLens);

            CollectionAssert.AreEqual(new[] { ItemType.ExtraDie, ItemType.PeekLens },
                CreateAI().ItemsBeforeDeciding(match));
        }

        [Test]
        public void ItemsBeforeDeciding_PeekAlreadyPending_KeepsSpareLens()
        {
            var match = CreateMatch(WeakMonsterRolls, ItemType.PeekLens, ItemType.PeekLens);
            match.UseItem(Side.Monster, ItemType.PeekLens);

            CollectionAssert.IsEmpty(CreateAI().ItemsBeforeDeciding(match));
        }

        [Test]
        public void ItemsBeforeDeciding_OpeningBid_DoesNotReroll()
        {
            var match = CreateMatch(WeakMonsterRolls, ItemType.Reroll);

            CollectionAssert.IsEmpty(CreateAI().ItemsBeforeDeciding(match));
        }

        [Test]
        public void ItemsBeforeDeciding_HandDoesNotSupportPlausibleBid_Rerolls()
        {
            var match = CreateMatch(WeakMonsterRolls, ItemType.Reroll);
            match.PlaceBid(Side.Monster, new Bid(1, 6));
            match.PlaceBid(Side.Player, new Bid(2, 2));

            CollectionAssert.AreEqual(new[] { ItemType.Reroll }, CreateAI().ItemsBeforeDeciding(match));
        }

        [Test]
        public void ItemsAfterDeciding_SureChallenge_ArmsStealOxygen()
        {
            var match = CreateMatch(WeakMonsterRolls, ItemType.StealOxygen, ItemType.SealTape);
            match.PlaceBid(Side.Monster, new Bid(1, 3));
            match.PlaceBid(Side.Player, new Bid(9, 4));
            var ai = CreateAI();

            var decision = ai.Decide(match);

            Assert.IsTrue(decision.IsChallenge);
            CollectionAssert.AreEqual(new[] { ItemType.StealOxygen }, ai.ItemsAfterDeciding(match, decision));
        }

        [Test]
        public void ItemsAfterDeciding_SafeRaise_ArmsStealAndSealsPlayer()
        {
            var match = CreateMatch(StrongMonsterRolls, ItemType.StealOxygen, ItemType.SealTape);

            var items = CreateAI().ItemsAfterDeciding(match, MonsterDecision.Raise(new Bid(3, 4)));

            CollectionAssert.AreEqual(new[] { ItemType.StealOxygen, ItemType.SealTape }, items);
        }

        [Test]
        public void ItemsAfterDeciding_RiskyRaise_KeepsItems()
        {
            var match = CreateMatch(WeakMonsterRolls, ItemType.StealOxygen, ItemType.SealTape);

            CollectionAssert.IsEmpty(CreateAI().ItemsAfterDeciding(match, MonsterDecision.Raise(new Bid(6, 4))));
        }

        [Test]
        public void Decide_AfterSealingPlayer_RaisesInsteadOfChallengingOwnBid()
        {
            var match = CreateMatch(StrongMonsterRolls, ItemType.SealTape);
            match.UseItem(Side.Monster, ItemType.SealTape);
            match.PlaceBid(Side.Monster, new Bid(3, 4));

            var decision = CreateAI().Decide(match);

            Assert.IsTrue(match.MustRaise);
            Assert.IsFalse(decision.IsChallenge);
            Assert.AreEqual(BidValidation.Valid, match.PlaceBid(Side.Monster, decision.Bid));
        }

        [Test]
        public void Probability_PeekShowsNoneOfFace_RulesItOutForUnknownDice()
        {
            var peek = new PeekResult(4, 0);

            Assert.AreEqual(0.0, CreateAI().Probability(new[] { 2, 3, 5, 6, 6 }, 5, new Bid(1, 4), false, peek));
        }

        [Test]
        public void Probability_PeekShowsEnoughOfFace_IsCertain()
        {
            var peek = new PeekResult(4, 2);

            Assert.AreEqual(1.0, CreateAI().Probability(new[] { 2, 3, 5, 6, 6 }, 5, new Bid(2, 4), false, peek));
        }

        [Test]
        public void Probability_PeekOnOtherFace_NarrowsRemainingDice()
        {
            var peek = new PeekResult(6, 4);

            Assert.AreEqual(0.2, CreateAI().Probability(new[] { 2, 3, 5, 6, 6 }, 5, new Bid(1, 4), false, peek), 1e-9);
        }
    }
}
