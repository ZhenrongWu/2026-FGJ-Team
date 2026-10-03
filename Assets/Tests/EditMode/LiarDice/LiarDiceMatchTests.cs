using System;
using FGJ.LiarDice;
using NUnit.Framework;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class LiarDiceMatchTests
    {
        private static readonly int[] RoundOneDice = { 1, 4, 4, 2, 6, 3, 4, 5, 5, 1 };

        private static LiarDiceMatch CreateMatch(int oxygen = 2, params int[] extraRolls)
        {
            var rolls = new int[RoundOneDice.Length + extraRolls.Length];
            RoundOneDice.CopyTo(rolls, 0);
            extraRolls.CopyTo(rolls, RoundOneDice.Length);
            var match = new LiarDiceMatch(new MatchSettings(5, 5, oxygen, oxygen), new FixedDiceRoller(rolls));
            match.Start();
            return match;
        }

        [Test]
        public void Start_RollsDiceAndMonsterBidsFirst()
        {
            var match = CreateMatch();

            Assert.AreEqual(MatchPhase.Bidding, match.Phase);
            Assert.AreEqual(Side.Monster, match.CurrentTurn);
            CollectionAssert.AreEqual(new[] { 1, 4, 4, 2, 6 }, match.GetDice(Side.Player));
            CollectionAssert.AreEqual(new[] { 3, 4, 5, 5, 1 }, match.GetDice(Side.Monster));
            Assert.IsNull(match.CurrentBid);
            Assert.IsFalse(match.CanChallenge);
        }

        [Test]
        public void PlaceBid_Valid_PassesTurnToOpponent()
        {
            var match = CreateMatch();

            Assert.AreEqual(BidValidation.Valid, match.PlaceBid(Side.Monster, new Bid(3, 4)));
            Assert.AreEqual(new Bid(3, 4), match.CurrentBid);
            Assert.AreEqual(Side.Monster, match.CurrentBidder);
            Assert.AreEqual(Side.Player, match.CurrentTurn);
        }

        [Test]
        public void PlaceBid_NotHigher_IsRejectedAndStateUnchanged()
        {
            var match = CreateMatch();
            match.PlaceBid(Side.Monster, new Bid(3, 4));

            Assert.AreEqual(BidValidation.NotHigher, match.PlaceBid(Side.Player, new Bid(3, 2)));
            Assert.AreEqual(new Bid(3, 4), match.CurrentBid);
            Assert.AreEqual(Side.Player, match.CurrentTurn);
        }

        [Test]
        public void PlaceBid_OutOfTurn_Throws()
        {
            var match = CreateMatch();
            Assert.Throws<InvalidOperationException>(() => match.PlaceBid(Side.Player, new Bid(1, 2)));
        }

        [Test]
        public void Challenge_WithoutBid_Throws()
        {
            var match = CreateMatch();
            Assert.Throws<InvalidOperationException>(() => match.Challenge(Side.Monster));
        }

        [Test]
        public void Challenge_BidTrue_ChallengerLosesOxygen()
        {
            var match = CreateMatch();
            match.PlaceBid(Side.Monster, new Bid(4, 4));

            var result = match.Challenge(Side.Player);

            Assert.AreEqual(5, result.ActualCount);
            Assert.AreEqual(Side.Player, result.Loser);
            Assert.IsFalse(result.ChallengerWon);
            Assert.AreEqual(1, match.PlayerOxygen);
            Assert.AreEqual(2, match.MonsterOxygen);
            Assert.AreEqual(MatchPhase.RoundOver, match.Phase);
        }

        [Test]
        public void Challenge_ExactCount_ChallengerLoses()
        {
            var match = CreateMatch();
            match.PlaceBid(Side.Monster, new Bid(5, 4));

            Assert.AreEqual(Side.Player, match.Challenge(Side.Player).Loser);
        }

        [Test]
        public void Challenge_BidFalse_BidderLosesOxygen()
        {
            var match = CreateMatch();
            match.PlaceBid(Side.Monster, new Bid(6, 4));

            var result = match.Challenge(Side.Player);

            Assert.AreEqual(Side.Monster, result.Loser);
            Assert.IsTrue(result.ChallengerWon);
            Assert.AreEqual(1, match.MonsterOxygen);
        }

        [Test]
        public void BiddingOnes_CancelsWildForRestOfRound()
        {
            var match = CreateMatch();
            match.PlaceBid(Side.Monster, new Bid(2, 1));
            Assert.IsFalse(match.WildActive);

            match.PlaceBid(Side.Player, new Bid(4, 4));
            var result = match.Challenge(Side.Monster);

            Assert.AreEqual(3, result.ActualCount);
            Assert.AreEqual(Side.Player, result.Loser);
        }

        [Test]
        public void NextRound_RerollsResetsWildAndLoserBidsFirst()
        {
            var match = CreateMatch(2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3);
            match.PlaceBid(Side.Monster, new Bid(2, 1));
            match.Challenge(Side.Player);

            match.NextRound();

            Assert.AreEqual(Side.Player, match.CurrentTurn);
            Assert.IsTrue(match.WildActive);
            Assert.IsNull(match.CurrentBid);
            CollectionAssert.AreEqual(new[] { 2, 2, 2, 2, 2 }, match.GetDice(Side.Player));
            CollectionAssert.AreEqual(new[] { 3, 3, 3, 3, 3 }, match.GetDice(Side.Monster));
        }

        [Test]
        public void OxygenDepleted_EndsMatchWithWinner()
        {
            var match = CreateMatch(1);
            Side? winner = null;
            match.MatchEnded += side => winner = side;
            match.PlaceBid(Side.Monster, new Bid(6, 4));

            match.Challenge(Side.Player);

            Assert.AreEqual(MatchPhase.MatchOver, match.Phase);
            Assert.AreEqual(Side.Player, match.Winner);
            Assert.AreEqual(Side.Player, winner);
            Assert.Throws<InvalidOperationException>(() => match.NextRound());
        }

        [Test]
        public void CanRaise_FalseAtMaximumBid()
        {
            var match = CreateMatch();
            match.PlaceBid(Side.Monster, new Bid(10, 6));

            Assert.IsFalse(match.CanRaise);
            Assert.IsTrue(match.CanChallenge);
        }
    }
}
