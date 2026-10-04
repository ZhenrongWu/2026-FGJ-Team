using System.Linq;
using FGJ.LiarDice;
using FGJ.LiarDice.Table;
using FGJ.LiarDice.UI;
using NUnit.Framework;
using UnityEngine;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class ItemPanelRulesTests
    {
        private readonly BidPanelRules _rules = new BidPanelRules();

        private static LiarDiceMatch StartMatch(params ItemType[] playerItems)
        {
            var rolls = Enumerable.Repeat(3, 10).ToArray();
            var dealt = playerItems.Concat(Enumerable.Repeat(ItemType.Reroll, playerItems.Length)).ToArray();
            var match = new LiarDiceMatch(new MatchSettings(5, 5, 2, 2, Side.Monster, null, playerItems.Length),
                new FixedDiceRoller(rolls), new FixedItemDealer(dealt));
            match.Start();
            return match;
        }

        [Test]
        public void PlayerTurnWithItems_EnablesItems()
        {
            var match = StartMatch(ItemType.SealTape);
            match.PlaceBid(Side.Monster, new Bid(2, 3));

            Assert.IsTrue(_rules.Evaluate(match, false, false).ItemsEnabled);
        }

        [Test]
        public void PlayerTurnWithoutItems_DisablesItems()
        {
            var match = StartMatch();
            match.PlaceBid(Side.Monster, new Bid(2, 3));

            Assert.IsFalse(_rules.Evaluate(match, false, false).ItemsEnabled);
        }

        [Test]
        public void MonsterTurnOrBusy_DisablesItems()
        {
            var match = StartMatch(ItemType.SealTape);

            Assert.IsFalse(_rules.Evaluate(match, false, false).ItemsEnabled);
            match.PlaceBid(Side.Monster, new Bid(2, 3));
            Assert.IsFalse(_rules.Evaluate(match, false, true).ItemsEnabled);
        }

        [Test]
        public void PlayerSealedMonster_MustTypeHigherBidWithoutBluffing()
        {
            var match = StartMatch(ItemType.SealTape);
            match.PlaceBid(Side.Monster, new Bid(2, 3));
            match.UseItem(Side.Player, ItemType.SealTape);
            match.PlaceBid(Side.Player, new Bid(3, 3));

            var state = _rules.Evaluate(match, false, false);

            Assert.AreEqual(Side.Player, match.CurrentTurn);
            Assert.IsFalse(state.BelieveEnabled);
            Assert.IsFalse(state.BluffEnabled);
            Assert.IsTrue(state.BidInputEnabled);
        }
    }

    public class SegmentFitterTests
    {
        private readonly SegmentFitter _fitter = new SegmentFitter();

        [Test]
        public void FewSegments_KeepPreferredSize()
        {
            var fit = _fitter.Fit(232f, 44f, 8f, 2);

            Assert.AreEqual(44f, fit.Height, 1e-4f);
            Assert.AreEqual(8f, fit.Spacing, 1e-4f);
        }

        [TestCase(5)]
        [TestCase(8)]
        [TestCase(16)]
        public void ManySegments_ShrinkToFitAvailableHeight(int count)
        {
            var fit = _fitter.Fit(232f, 44f, 8f, count);

            Assert.LessOrEqual(fit.Height * count + fit.Spacing * (count - 1), 232f + 1e-3f);
            Assert.Greater(fit.Height, 0f);
        }
    }

    public class TrayDieScaleTests
    {
        private const float DieSize = 0.08f;
        private const float TrayRadius = 0.25f;
        private readonly TrayLayout _layout = new TrayLayout();

        [TestCase(5)]
        [TestCase(10)]
        public void UpToTenDice_KeepFullSize(int count)
        {
            Assert.AreEqual(1f, _layout.DieScale(_layout.Positions(count, TrayRadius), TrayRadius, DieSize), 1e-4f);
        }

        [TestCase(15)]
        [TestCase(16)]
        [TestCase(19)]
        public void MoreDice_ShrinkUntilNoneOverlapOrLeaveTheTray(int count)
        {
            var positions = _layout.Positions(count, TrayRadius);
            var scale = _layout.DieScale(positions, TrayRadius, DieSize);
            var size = DieSize * scale;

            Assert.Less(scale, 1f);
            Assert.Greater(scale, 0.4f);
            foreach (var position in positions)
                Assert.LessOrEqual(position.magnitude + size * 0.71f, TrayRadius + 1e-4f);
            for (var i = 0; i < positions.Count; i++)
            for (var j = i + 1; j < positions.Count; j++)
                Assert.GreaterOrEqual(Vector3.Distance(positions[i], positions[j]), size * 1.05f - 1e-4f, $"{i}-{j}");
        }
    }

    public class ItemTextTests
    {
        private readonly LiarDiceText _text = new LiarDiceText();

        [TestCase(ItemType.PeekLens, ExpectedResult = "偷窺鏡片")]
        [TestCase(ItemType.ExtraDie, ExpectedResult = "偷加骰子")]
        [TestCase(ItemType.Reroll, ExpectedResult = "重搖")]
        [TestCase(ItemType.SealTape, ExpectedResult = "封口膠帶")]
        [TestCase(ItemType.StealOxygen, ExpectedResult = "偷走氧氣")]
        public string ItemName_MatchesDesignDocument(ItemType item) => _text.ItemName(item);

        [Test]
        public void MonsterExtraDie_IsNotAnnouncedButOtherItemsAre()
        {
            Assert.IsFalse(_text.AnnouncesUse(Side.Monster, ItemType.ExtraDie));
            Assert.IsTrue(_text.AnnouncesUse(Side.Player, ItemType.ExtraDie));
            Assert.IsTrue(_text.AnnouncesUse(Side.Monster, ItemType.SealTape));
        }

        [Test]
        public void PeekReveal_DescribesMonsterFaceCount()
        {
            Assert.AreEqual("偷窺鏡片：怪物有 2 顆 5 點。", _text.PeekReveal(new PeekResult(5, 2)));
        }

        [Test]
        public void RoundResult_OxygenStolen_MentionsExtraLoss()
        {
            var result = new RoundResult(Side.Player, Side.Monster, new Bid(6, 4), 2, Side.Monster, true);

            StringAssert.EndsWith("你偷走一格氧氣，怪物再扣一格！", _text.RoundResult(result, false));
        }

        [Test]
        public void TurnStatus_PlayerMustRaiseAfterSealing_ExplainsWhy()
        {
            var match = new LiarDiceMatch(new MatchSettings(5, 5, 2, 2, Side.Monster, null, 1),
                new FixedDiceRoller(Enumerable.Repeat(3, 10).ToArray()),
                new FixedItemDealer(ItemType.SealTape, ItemType.Reroll));
            match.Start();
            match.PlaceBid(Side.Monster, new Bid(2, 3));
            match.UseItem(Side.Player, ItemType.SealTape);
            match.PlaceBid(Side.Player, new Bid(3, 3));

            Assert.AreEqual("怪物被封口了：再喊一次更大的數", _text.TurnStatus(match));
        }
    }
}
