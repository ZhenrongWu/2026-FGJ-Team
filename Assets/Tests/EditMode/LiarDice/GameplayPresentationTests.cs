using System.Linq;
using FGJ.LiarDice;
using FGJ.LiarDice.Table;
using FGJ.LiarDice.UI;
using NUnit.Framework;
using UnityEngine;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class BidPanelRulesTests
    {
        private readonly BidPanelRules _rules = new BidPanelRules();

        private static LiarDiceMatch StartMatch(Side firstTurn, int diceEach = 5)
        {
            var rolls = Enumerable.Repeat(3, diceEach * 2).ToArray();
            var match = new LiarDiceMatch(new MatchSettings(diceEach, diceEach, 2, 2, firstTurn),
                new FixedDiceRoller(rolls));
            match.Start();
            return match;
        }

        [Test]
        public void PlayerOpensRound_OnlyBidInputEnabled()
        {
            var state = _rules.Evaluate(StartMatch(Side.Player), false, false);

            Assert.IsFalse(state.BelieveEnabled);
            Assert.IsFalse(state.BluffEnabled);
            Assert.IsTrue(state.BidInputEnabled);
            Assert.IsFalse(state.ContinueVisible);
        }

        [Test]
        public void AnsweringBid_BeforeBelieving_CanBelieveOrBluffButNotType()
        {
            var match = StartMatch(Side.Monster);
            match.PlaceBid(Side.Monster, new Bid(3, 4));

            var state = _rules.Evaluate(match, false, false);

            Assert.IsTrue(state.BelieveEnabled);
            Assert.IsTrue(state.BluffEnabled);
            Assert.IsFalse(state.BidInputEnabled);
        }

        [Test]
        public void AnsweringBid_AfterBelieving_CanTypeOrStillBluff()
        {
            var match = StartMatch(Side.Monster);
            match.PlaceBid(Side.Monster, new Bid(3, 4));

            var state = _rules.Evaluate(match, true, false);

            Assert.IsFalse(state.BelieveEnabled);
            Assert.IsTrue(state.BluffEnabled);
            Assert.IsTrue(state.BidInputEnabled);
        }

        [Test]
        public void MaximumBid_OnlyBluffRemains()
        {
            var match = StartMatch(Side.Monster, 1);
            match.PlaceBid(Side.Monster, new Bid(2, 6));

            var state = _rules.Evaluate(match, false, false);

            Assert.IsFalse(state.BelieveEnabled);
            Assert.IsTrue(state.BluffEnabled);
            Assert.IsFalse(state.BidInputEnabled);
        }

        [Test]
        public void MonsterTurnOrBusy_DisablesEverything()
        {
            Assert.AreEqual(default(BidPanelState), _rules.Evaluate(StartMatch(Side.Monster), false, false));
            Assert.AreEqual(default(BidPanelState), _rules.Evaluate(StartMatch(Side.Player), false, true));
        }

        [Test]
        public void RoundOver_ShowsContinueOnceAnimationsFinish()
        {
            var match = StartMatch(Side.Monster);
            match.PlaceBid(Side.Monster, new Bid(3, 4));
            match.Challenge(Side.Player);

            Assert.IsFalse(_rules.Evaluate(match, false, true).ContinueVisible);
            Assert.IsTrue(_rules.Evaluate(match, false, false).ContinueVisible);
        }
    }

    public class MatchLogTests
    {
        [Test]
        public void Add_KeepsOrderAndRaisesChanged()
        {
            var log = new MatchLog();
            var changes = 0;
            log.Changed += () => changes++;

            log.Add("a", LogKind.System);
            log.Add("b", LogKind.PlayerAction);

            CollectionAssert.AreEqual(new[] { "a", "b" }, log.Entries.Select(entry => entry.Text));
            Assert.AreEqual(LogKind.PlayerAction, log.Entries[1].Kind);
            Assert.AreEqual(2, changes);
        }

        [Test]
        public void Add_BeyondCapacity_DropsOldestEntries()
        {
            var log = new MatchLog(3);
            foreach (var text in new[] { "1", "2", "3", "4", "5" })
                log.Add(text, LogKind.System);

            CollectionAssert.AreEqual(new[] { "3", "4", "5" }, log.Entries.Select(entry => entry.Text));
        }

        [Test]
        public void Clear_RemovesEverything()
        {
            var log = new MatchLog();
            log.Add("a", LogKind.System);

            log.Clear();

            Assert.IsEmpty(log.Entries);
        }
    }

    public class DieFaceLayoutTests
    {
        private readonly DieFaceLayout _layout = new DieFaceLayout();

        [Test]
        public void OppositeFaces_SumToSeven()
        {
            for (var value = 1; value <= 6; value++)
                Assert.AreEqual(-_layout.NormalFor(value), _layout.NormalFor(7 - value));
        }

        [Test]
        public void RotationShowing_PutsRequestedFaceUpForAnyYaw()
        {
            foreach (var yaw in new[] { 0f, 37f, 145f, 290f })
            {
                for (var value = 1; value <= 6; value++)
                    Assert.AreEqual(value, _layout.ValueFacingUp(_layout.RotationShowing(value, yaw)),
                        $"value {value} yaw {yaw}");
            }
        }

        [Test]
        public void NormalFor_OutOfRange_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _layout.NormalFor(7));
        }
    }

    public class TrayLayoutTests
    {
        private const float DieSize = 0.08f;
        private const float TrayRadius = 0.25f;
        private readonly TrayLayout _layout = new TrayLayout();

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(8)]
        [TestCase(10)]
        public void Positions_FitInsideTrayWithoutOverlapping(int count)
        {
            var positions = _layout.Positions(count, TrayRadius);

            Assert.AreEqual(count, positions.Count);
            foreach (var position in positions)
                Assert.LessOrEqual(position.magnitude + DieSize * 0.71f, TrayRadius);
            for (var i = 0; i < positions.Count; i++)
            for (var j = i + 1; j < positions.Count; j++)
                Assert.Greater(Vector3.Distance(positions[i], positions[j]), DieSize * 1.05f, $"{i}-{j}");
        }

        [Test]
        public void Positions_ZeroCount_IsEmpty()
        {
            Assert.IsEmpty(_layout.Positions(0, TrayRadius));
        }
    }
}
