using FGJ.LiarDice;
using NUnit.Framework;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class LiarDiceRulesTests
    {
        private readonly LiarDiceRules _rules = new LiarDiceRules();

        [TestCase(4, 2, 3, 6, ExpectedResult = true, TestName = "數量變多即使點數變小也算更大")]
        [TestCase(3, 6, 3, 4, ExpectedResult = true, TestName = "同數量點數變大")]
        [TestCase(3, 2, 3, 4, ExpectedResult = false, TestName = "同數量點數變小")]
        [TestCase(3, 4, 3, 4, ExpectedResult = false, TestName = "完全相同")]
        [TestCase(3, 1, 3, 6, ExpectedResult = false, TestName = "1 點最小，3個6 之後不能喊 3個1")]
        [TestCase(4, 1, 3, 6, ExpectedResult = true, TestName = "3個6 之後可以喊 4個1")]
        public bool IsHigherThan(int quantity, int face, int otherQuantity, int otherFace)
        {
            return new Bid(quantity, face).IsHigherThan(new Bid(otherQuantity, otherFace));
        }

        [Test]
        public void CountMatching_WildActive_CountsOnesAsAnyFace()
        {
            Assert.AreEqual(4, _rules.CountMatching(new[] { 1, 4, 4, 1, 6 }, 4, true));
        }

        [Test]
        public void CountMatching_WildCancelled_OnesOnlyCountAsOnes()
        {
            Assert.AreEqual(2, _rules.CountMatching(new[] { 1, 4, 4, 1, 6 }, 4, false));
        }

        [Test]
        public void CountMatching_FaceOne_CountsOnlyOnes()
        {
            Assert.AreEqual(2, _rules.CountMatching(new[] { 1, 4, 4, 1, 6 }, 1, true));
        }

        [Test]
        public void Validate_FirstBid_AnyInRangeIsValid()
        {
            Assert.AreEqual(BidValidation.Valid, _rules.Validate(null, new Bid(1, 1), 10));
        }

        [TestCase(3, 0, ExpectedResult = BidValidation.FaceOutOfRange)]
        [TestCase(3, 7, ExpectedResult = BidValidation.FaceOutOfRange)]
        [TestCase(0, 3, ExpectedResult = BidValidation.QuantityTooLow)]
        [TestCase(11, 3, ExpectedResult = BidValidation.QuantityTooHigh)]
        [TestCase(3, 3, ExpectedResult = BidValidation.NotHigher)]
        [TestCase(3, 5, ExpectedResult = BidValidation.Valid)]
        [TestCase(10, 2, ExpectedResult = BidValidation.Valid)]
        public BidValidation Validate_AgainstCurrentBid(int quantity, int face)
        {
            return _rules.Validate(new Bid(3, 4), new Bid(quantity, face), 10);
        }

        [Test]
        public void CanRaise_FalseOnlyAtMaxQuantityAndMaxFace()
        {
            Assert.IsTrue(_rules.CanRaise(null, 10));
            Assert.IsTrue(_rules.CanRaise(new Bid(9, 6), 10));
            Assert.IsTrue(_rules.CanRaise(new Bid(10, 5), 10));
            Assert.IsFalse(_rules.CanRaise(new Bid(10, 6), 10));
        }

        [Test]
        public void OnesNotWild_NeverCountOnesForOtherFacesAndBiddingOnesKeepsNothingToCancel()
        {
            var rules = new LiarDiceRules(onesAreWild: false);

            Assert.AreEqual(2, rules.CountMatching(new[] { 1, 4, 4, 1, 6 }, 4, true));
            Assert.IsFalse(rules.CancelsWild(new Bid(2, 1)));
            Assert.IsFalse(rules.IsWildFor(4, true));
        }

        [Test]
        public void Opponent_SwapsSides()
        {
            Assert.AreEqual(Side.Monster, Side.Player.Opponent());
            Assert.AreEqual(Side.Player, Side.Monster.Opponent());
        }
    }
}
