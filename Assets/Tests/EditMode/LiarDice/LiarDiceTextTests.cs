using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using NUnit.Framework;

namespace FGJ.Tests.EditMode.LiarDice
{
    public class LiarDiceTextTests
    {
        private readonly LiarDiceText _text = new LiarDiceText();

        [Test]
        public void CurrentBid_NoBid_ShowsNotYetBid()
        {
            Assert.AreEqual("尚未喊數", _text.CurrentBid(null, null));
        }

        [Test]
        public void CurrentBid_WithBidder_ShowsWhoBidWhat()
        {
            Assert.AreEqual("怪物喊：3 個 4 點", _text.CurrentBid(new Bid(3, 4), Side.Monster));
            Assert.AreEqual("你喊：5 個 1 點", _text.CurrentBid(new Bid(5, 1), Side.Player));
        }

        [Test]
        public void RoundResult_WildActiveNonOneFace_MentionsWild()
        {
            var result = new RoundResult(Side.Player, Side.Monster, new Bid(6, 4), 5, Side.Monster);

            Assert.AreEqual("你喊吹牛，開盅「6 個 4 點」：全場 5 顆（含萬用 1 點）。怪物輸了，扣一格氧氣。",
                _text.RoundResult(result, true));
        }

        [Test]
        public void RoundResult_WildCancelled_OmitsWildNote()
        {
            var result = new RoundResult(Side.Monster, Side.Player, new Bid(3, 4), 3, Side.Monster);

            Assert.AreEqual("怪物喊吹牛，開盅「3 個 4 點」：全場 3 顆。怪物輸了，扣一格氧氣。",
                _text.RoundResult(result, false));
        }

        [Test]
        public void TurnStatus_PlayerAtMaximumBid_SaysOnlyBluff()
        {
            var match = new LiarDiceMatch(new MatchSettings(1, 1, 2, 2), new FixedDiceRoller(2, 3));
            match.Start();
            match.PlaceBid(Side.Monster, new Bid(2, 6));

            Assert.AreEqual("已經喊到最大，只能喊吹牛開盅", _text.TurnStatus(match));
        }

        [Test]
        public void TurnStatus_PlayerOpensRound_AsksForFirstBid()
        {
            var match = new LiarDiceMatch(new MatchSettings(1, 1, 2, 2, Side.Player), new FixedDiceRoller(2, 3));
            match.Start();

            Assert.AreEqual("輪到你先喊數", _text.TurnStatus(match));
        }

        [Test]
        public void PlayerBid_DistinguishesBelievingFromOpening()
        {
            Assert.AreEqual("你相信，喊：4 個 4 點", _text.PlayerBid(new Bid(4, 4), true));
            Assert.AreEqual("你喊：2 個 3 點", _text.PlayerBid(new Bid(2, 3), false));
        }

        [Test]
        public void SpeechAndRoundStart_FormatLogLines()
        {
            Assert.AreEqual("沼澤看守者：「開！」", _text.Speech("沼澤看守者", "「開！」"));
            Assert.AreEqual("— 第 2 局，怪物先喊 —", _text.RoundStart(2, Side.Monster));
        }

        [Test]
        public void MatchOver_DescribesWinner()
        {
            Assert.AreEqual("怪物的氧氣耗盡，你贏了！", _text.MatchOver(Side.Player));
            Assert.AreEqual("你的氧氣耗盡了……", _text.MatchOver(Side.Monster));
        }
    }
}
