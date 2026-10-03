using FGJ.LiarDice;
using NUnit.Framework;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class BidInputValidatorTests
    {
        private static readonly Bid Current = new Bid(3, 4);

        [TestCase("", "4", "請輸入數量。")]
        [TestCase("  ", "4", "請輸入數量。")]
        [TestCase("4", "", "請輸入點數。")]
        [TestCase("abc", "4", "數量必須是整數。")]
        [TestCase("4", "4.5", "點數必須是整數。")]
        [TestCase("4", "7", "點數必須介於 1～6。")]
        [TestCase("0", "4", "數量至少要 1。")]
        [TestCase("11", "4", "數量不能超過場上骰子總數（10）。")]
        [TestCase("3", "2", "必須比目前的「3 個 4 點」大：數量更多，或同數量點數更大。")]
        public void InvalidInput_ReturnsMessage(string quantity, string face, string expectedMessage)
        {
            var result = BidInputValidator.Validate(quantity, face, Current, 10);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(expectedMessage, result.ErrorMessage);
        }

        [Test]
        public void NullInput_ReturnsMessage()
        {
            Assert.IsFalse(BidInputValidator.Validate(null, null, null, 10).IsValid);
        }

        [Test]
        public void ValidInput_TrimsAndReturnsBid()
        {
            var result = BidInputValidator.Validate(" 4 ", " 2", Current, 10);

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(new Bid(4, 2), result.Bid);
            Assert.IsNull(result.ErrorMessage);
        }
    }
}
