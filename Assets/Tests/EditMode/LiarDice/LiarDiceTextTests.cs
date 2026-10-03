using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using NUnit.Framework;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class LiarDiceTextTests
    {
        [Test]
        public void CurrentBid_NoBid_ShowsNotYetBid()
        {
            Assert.AreEqual("尚未喊數", LiarDiceText.CurrentBid(null, null));
        }

        [Test]
        public void CurrentBid_WithBidder_ShowsWhoBidWhat()
        {
            Assert.AreEqual("怪物喊：3 個 4 點", LiarDiceText.CurrentBid(new Bid(3, 4), Side.Monster));
            Assert.AreEqual("你喊：5 個 1 點", LiarDiceText.CurrentBid(new Bid(5, 1), Side.Player));
        }

        [Test]
        public void Oxygen_ShowsFilledAndEmptyPips()
        {
            Assert.AreEqual("你的氧氣 ●○", LiarDiceText.Oxygen("你的", 1, 2));
            Assert.AreEqual("怪物氧氣 ○○", LiarDiceText.Oxygen("怪物", -1, 2));
        }

        [Test]
        public void RoundResult_WildActiveNonOneFace_MentionsWild()
        {
            var result = new RoundResult(Side.Player, Side.Monster, new Bid(6, 4), 5, Side.Monster);

            Assert.AreEqual("你質疑「6 個 4 點」\n全場符合 5 顆（含萬用 1 點）\n怪物輸了，扣一格氧氣。",
                LiarDiceText.RoundResult(result, true));
        }

        [Test]
        public void RoundResult_WildCancelled_OmitsWildNote()
        {
            var result = new RoundResult(Side.Monster, Side.Player, new Bid(3, 4), 3, Side.Monster);

            Assert.AreEqual("怪物質疑「3 個 4 點」\n全場符合 3 顆\n怪物輸了，扣一格氧氣。",
                LiarDiceText.RoundResult(result, false));
        }

        [Test]
        public void TurnStatus_PlayerAtMaximumBid_SaysOnlyChallenge()
        {
            var match = new LiarDiceMatch(new MatchSettings(1, 1, 2, 2), new FixedDiceRoller(2, 3));
            match.Start();
            match.PlaceBid(Side.Monster, new Bid(2, 6));

            Assert.AreEqual("已經喊到最大，只能質疑", LiarDiceText.TurnStatus(match));
        }

        [Test]
        public void MatchOver_DescribesWinner()
        {
            Assert.AreEqual("怪物的氧氣耗盡，你贏了！", LiarDiceText.MatchOver(Side.Player));
            Assert.AreEqual("你的氧氣耗盡了……", LiarDiceText.MatchOver(Side.Monster));
        }
    }
}
