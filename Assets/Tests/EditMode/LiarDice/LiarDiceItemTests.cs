using System;
using System.Linq;
using FGJ.LiarDice;
using NUnit.Framework;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class LiarDiceItemTests
    {
        private static readonly int[] PlayerDice = { 1, 4, 4, 2, 6 };
        private static readonly int[] MonsterDice = { 3, 4, 5, 5, 1 };

        private static LiarDiceMatch CreateMatch(ItemType[] playerItems, ItemType[] monsterItems,
            int[] extraRolls = null, int playerOxygen = 3, int monsterOxygen = 3, int playerMaxOxygen = 5)
        {
            var rolls = PlayerDice.Concat(MonsterDice).Concat(extraRolls ?? new int[0]).ToArray();
            var itemCount = Math.Max(playerItems.Length, monsterItems.Length);
            var dealt = PadWithRerolls(playerItems, itemCount).Concat(PadWithRerolls(monsterItems, itemCount)).ToArray();
            var settings = new MatchSettings(5, 5, playerOxygen, monsterOxygen, Side.Monster, null, itemCount,
                playerMaxOxygen);
            var match = new LiarDiceMatch(settings, new FixedDiceRoller(rolls), new FixedItemDealer(dealt));
            match.Start();
            return match;
        }

        private static ItemType[] Items(params ItemType[] items) => items;

        private static ItemType[] PadWithRerolls(ItemType[] items, int count)
        {
            return items.Concat(Enumerable.Repeat(ItemType.Reroll, count - items.Length)).ToArray();
        }

        [Test]
        public void Start_DealsItemCountToEachSideAllowingDuplicates()
        {
            var match = CreateMatch(Items(ItemType.Reroll, ItemType.Reroll), Items(ItemType.SealTape, ItemType.PeekLens));

            CollectionAssert.AreEqual(new[] { ItemType.Reroll, ItemType.Reroll }, match.GetItems(Side.Player));
            CollectionAssert.AreEqual(new[] { ItemType.SealTape, ItemType.PeekLens }, match.GetItems(Side.Monster));
        }

        [Test]
        public void Start_WithoutItemCount_DealsNothing()
        {
            var match = CreateMatch(Items(), Items());

            Assert.IsEmpty(match.GetItems(Side.Player));
            Assert.IsEmpty(match.GetItems(Side.Monster));
        }

        [Test]
        public void UseItem_NotHeld_ReturnsNotHeldAndKeepsState()
        {
            var match = CreateMatch(Items(ItemType.Reroll), Items(ItemType.Reroll));

            Assert.AreEqual(ItemUseResult.NotHeld, match.UseItem(Side.Monster, ItemType.SealTape));
            Assert.AreEqual(1, match.GetItems(Side.Monster).Count);
        }

        [Test]
        public void UseItem_OutOfTurn_Throws()
        {
            var match = CreateMatch(Items(ItemType.Reroll), Items(ItemType.Reroll));

            Assert.Throws<InvalidOperationException>(() => match.UseItem(Side.Player, ItemType.Reroll));
        }

        [Test]
        public void UseItem_SeveralInOneTurn_AllConsumed()
        {
            var match = CreateMatch(Items(), Items(ItemType.StealOxygen, ItemType.SealTape), new[] { 6 });

            Assert.AreEqual(ItemUseResult.Used, match.UseItem(Side.Monster, ItemType.StealOxygen));
            Assert.AreEqual(ItemUseResult.Used, match.UseItem(Side.Monster, ItemType.SealTape));
            Assert.IsEmpty(match.GetItems(Side.Monster));
        }

        [Test]
        public void PeekLens_RevealsOpponentFaceCountAtNextRoundStart()
        {
            var nextRound = new[] { 2, 2, 2, 2, 2, 4, 4, 4, 1, 6 };
            var match = CreateMatch(Items(), Items(ItemType.PeekLens), nextRound.Concat(new[] { 2 }).ToArray());

            match.UseItem(Side.Monster, ItemType.PeekLens);
            Assert.IsTrue(match.HasPendingPeek(Side.Monster));
            Assert.IsNull(match.GetPeek(Side.Monster));
            match.PlaceBid(Side.Monster, new Bid(9, 6));
            match.Challenge(Side.Player);
            match.NextRound();

            Assert.IsFalse(match.HasPendingPeek(Side.Monster));
            Assert.AreEqual(new PeekResult(2, 5), match.GetPeek(Side.Monster));
            Assert.IsNull(match.GetPeek(Side.Player));
        }

        [Test]
        public void PeekLens_SecondUseWhilePending_IsAlreadyActive()
        {
            var match = CreateMatch(Items(), Items(ItemType.PeekLens, ItemType.PeekLens));

            match.UseItem(Side.Monster, ItemType.PeekLens);

            Assert.AreEqual(ItemUseResult.AlreadyActive, match.UseItem(Side.Monster, ItemType.PeekLens));
            Assert.AreEqual(1, match.GetItems(Side.Monster).Count);
        }

        [Test]
        public void ExtraDie_AddsHiddenDieThatCountsInChallenge()
        {
            var match = CreateMatch(Items(), Items(ItemType.ExtraDie), new[] { 4 });

            match.UseItem(Side.Monster, ItemType.ExtraDie);

            Assert.AreEqual(6, match.GetDice(Side.Monster).Count);
            Assert.AreEqual(6, match.VisibleDiceCount(Side.Monster, Side.Monster));
            Assert.AreEqual(5, match.VisibleDiceCount(Side.Player, Side.Monster));
            Assert.AreEqual(10, match.TotalDiceKnownTo(Side.Player));
            Assert.AreEqual(11, match.TotalDiceKnownTo(Side.Monster));

            match.PlaceBid(Side.Monster, new Bid(6, 4));
            var result = match.Challenge(Side.Player);

            Assert.AreEqual(6, result.ActualCount);
            Assert.AreEqual(Side.Player, result.Loser);
        }

        [Test]
        public void ExtraDie_StaysForLaterRoundsAndIsKnownAfterReveal()
        {
            var nextRound = Enumerable.Repeat(3, 11).ToArray();
            var match = CreateMatch(Items(), Items(ItemType.ExtraDie), new[] { 4 }.Concat(nextRound).ToArray());
            match.UseItem(Side.Monster, ItemType.ExtraDie);
            match.PlaceBid(Side.Monster, new Bid(2, 6));
            match.Challenge(Side.Player);

            match.NextRound();

            Assert.AreEqual(6, match.GetDice(Side.Monster).Count);
            Assert.AreEqual(6, match.VisibleDiceCount(Side.Player, Side.Monster));
        }

        [Test]
        public void BidAboveWhatPlayerCanSee_IsRejectedForPlayer()
        {
            var match = CreateMatch(Items(), Items(ItemType.ExtraDie), new[] { 4 });
            match.UseItem(Side.Monster, ItemType.ExtraDie);

            Assert.AreEqual(BidValidation.Valid, match.PlaceBid(Side.Monster, new Bid(3, 4)));
            Assert.AreEqual(BidValidation.QuantityTooHigh, match.PlaceBid(Side.Player, new Bid(11, 4)));
        }

        [Test]
        public void Reroll_RerollsBothCupsAndKeepsBidAndTurn()
        {
            var rerolled = new[] { 6, 6, 6, 6, 6, 2, 2, 2, 2, 2 };
            var match = CreateMatch(Items(ItemType.Reroll), Items(), rerolled);
            match.PlaceBid(Side.Monster, new Bid(3, 4));

            Assert.AreEqual(ItemUseResult.Used, match.UseItem(Side.Player, ItemType.Reroll));

            CollectionAssert.AreEqual(new[] { 6, 6, 6, 6, 6 }, match.GetDice(Side.Player));
            CollectionAssert.AreEqual(new[] { 2, 2, 2, 2, 2 }, match.GetDice(Side.Monster));
            Assert.AreEqual(new Bid(3, 4), match.CurrentBid);
            Assert.AreEqual(Side.Player, match.CurrentTurn);
            Assert.IsEmpty(match.GetItems(Side.Player));
        }

        [Test]
        public void SealTape_SkipsOpponentSoUserMustRaiseAgain()
        {
            var match = CreateMatch(Items(), Items(ItemType.SealTape));
            match.UseItem(Side.Monster, ItemType.SealTape);

            match.PlaceBid(Side.Monster, new Bid(2, 4));

            Assert.AreEqual(Side.Player, match.SkippedSide);
            Assert.AreEqual(Side.Monster, match.CurrentTurn);
            Assert.IsTrue(match.MustRaise);
            Assert.IsFalse(match.CanChallenge);
            Assert.Throws<InvalidOperationException>(() => match.Challenge(Side.Monster));
            Assert.AreEqual(BidValidation.NotHigher, match.PlaceBid(Side.Monster, new Bid(2, 3)));

            match.PlaceBid(Side.Monster, new Bid(3, 4));

            Assert.IsNull(match.SkippedSide);
            Assert.AreEqual(Side.Player, match.CurrentTurn);
            Assert.IsTrue(match.CanChallenge);
        }

        [Test]
        public void SealTape_OnMaximumBid_DoesNotSkipAndStaysPending()
        {
            var match = CreateMatch(Items(), Items(ItemType.SealTape));
            match.UseItem(Side.Monster, ItemType.SealTape);

            match.PlaceBid(Side.Monster, new Bid(10, 6));

            Assert.IsNull(match.SkippedSide);
            Assert.AreEqual(Side.Player, match.CurrentTurn);
            Assert.IsTrue(match.IsSealed(Side.Player));
        }

        [Test]
        public void SealTape_UnusedBeforeChallenge_SkipsOpponentsOpeningNextRound()
        {
            var nextRound = Enumerable.Repeat(3, 10).ToArray();
            var match = CreateMatch(Items(ItemType.SealTape), Items(), nextRound);
            match.PlaceBid(Side.Monster, new Bid(9, 6));
            match.UseItem(Side.Player, ItemType.SealTape);
            match.Challenge(Side.Player);

            match.NextRound();

            Assert.AreEqual(Side.Monster, match.SkippedSide);
            Assert.AreEqual(Side.Player, match.CurrentTurn);
            Assert.IsFalse(match.IsSealed(Side.Monster));
        }

        [Test]
        public void SealTape_WhileOpponentAlreadySealed_IsAlreadyActive()
        {
            var match = CreateMatch(Items(), Items(ItemType.SealTape, ItemType.SealTape));
            match.UseItem(Side.Monster, ItemType.SealTape);

            Assert.AreEqual(ItemUseResult.AlreadyActive, match.UseItem(Side.Monster, ItemType.SealTape));
        }

        [Test]
        public void StealOxygen_ArmedWinner_TakesExtraOxygenFromLoser()
        {
            var match = CreateMatch(Items(ItemType.StealOxygen), Items(), playerOxygen: 3, monsterOxygen: 4);
            match.PlaceBid(Side.Monster, new Bid(9, 4));
            match.UseItem(Side.Player, ItemType.StealOxygen);

            var result = match.Challenge(Side.Player);

            Assert.AreEqual(Side.Monster, result.Loser);
            Assert.IsTrue(result.OxygenStolen);
            Assert.AreEqual(2, match.MonsterOxygen);
            Assert.AreEqual(4, match.PlayerOxygen);
            Assert.IsFalse(match.IsStealArmed(Side.Player));
        }

        [Test]
        public void StealOxygen_ArmedSideLoses_ItemIsSpentWithoutEffect()
        {
            var match = CreateMatch(Items(ItemType.StealOxygen), Items(), playerOxygen: 3, monsterOxygen: 4);
            match.PlaceBid(Side.Monster, new Bid(2, 4));
            match.UseItem(Side.Player, ItemType.StealOxygen);

            var result = match.Challenge(Side.Player);

            Assert.AreEqual(Side.Player, result.Loser);
            Assert.IsFalse(result.OxygenStolen);
            Assert.AreEqual(2, match.PlayerOxygen);
            Assert.AreEqual(4, match.MonsterOxygen);
            Assert.IsEmpty(match.GetItems(Side.Player));
            Assert.IsFalse(match.IsStealArmed(Side.Player));
        }

        [Test]
        public void StealOxygen_WinnerAtMaximum_StaysAtMaximum()
        {
            var match = CreateMatch(Items(ItemType.StealOxygen), Items(), playerOxygen: 5, monsterOxygen: 4);
            match.PlaceBid(Side.Monster, new Bid(9, 4));
            match.UseItem(Side.Player, ItemType.StealOxygen);

            match.Challenge(Side.Player);

            Assert.AreEqual(5, match.PlayerOxygen);
            Assert.AreEqual(2, match.MonsterOxygen);
        }

        [Test]
        public void StealOxygen_LoserWithOneOxygen_EndsMatchAtZero()
        {
            var match = CreateMatch(Items(ItemType.StealOxygen), Items(), monsterOxygen: 1);
            match.PlaceBid(Side.Monster, new Bid(9, 4));
            match.UseItem(Side.Player, ItemType.StealOxygen);

            match.Challenge(Side.Player);

            Assert.AreEqual(0, match.MonsterOxygen);
            Assert.AreEqual(MatchPhase.MatchOver, match.Phase);
            Assert.AreEqual(Side.Player, match.Winner);
        }
    }
}
