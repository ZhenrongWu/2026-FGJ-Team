using System.Collections;
using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FGJ.Tests.PlayMode.LiarDice
{
    public class LiarDiceRoomTests
    {
        private static readonly int[] DiceSequence = { 1, 4, 4, 2, 6, 3, 4, 5, 5, 1 };

        private LiarDiceRoomController _controller;

        private LiarDiceView View => _controller.View;
        private LiarDiceMatch Match => _controller.Match;

        [SetUp]
        public void SetUp()
        {
            _controller = LiarDiceRoomBuilder.Build(null);
            _controller.SetMonsterThinkSeconds(0f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_controller.gameObject);
        }

        private void Begin(int oxygen = 2)
        {
            var profile = new MonsterAIProfile(0.35f, 0.5f, 0f, 2);
            _controller.Begin(new MatchSettings(5, 5, oxygen, oxygen), () => new CyclingDiceRoller(DiceSequence),
                new MonsterAI(profile, new System.Random(7)));
        }

        private static IEnumerator WaitFrames(int count)
        {
            for (var i = 0; i < count; i++)
                yield return null;
        }

        [UnityTest]
        public IEnumerator Begin_MonsterOpensRoundThenWaitsForPlayer()
        {
            Begin();
            Assert.AreEqual(Side.Monster, Match.CurrentTurn);
            Assert.IsFalse(View.IsInputInteractable);

            yield return WaitFrames(2);

            Assert.IsTrue(Match.CurrentBid.HasValue);
            Assert.IsTrue(_controller.IsPlayerTurn);
            Assert.IsTrue(View.IsInputInteractable);
            StringAssert.StartsWith("怪物喊：", View.CurrentBidLabel);
            Assert.AreEqual(LiarDiceText.MonsterBidLine(Match.CurrentBid.Value), View.MonsterLine);
        }

        [UnityTest]
        public IEnumerator SubmitPlayerBid_InvalidInput_ShowsErrorAndKeepsTurn()
        {
            Begin();
            yield return WaitFrames(2);
            var monsterBid = Match.CurrentBid;

            var result = _controller.SubmitPlayerBid("abc", "4");

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("數量必須是整數。", View.ErrorMessage);
            Assert.AreEqual(monsterBid, Match.CurrentBid);
            Assert.IsTrue(_controller.IsPlayerTurn);
        }

        [UnityTest]
        public IEnumerator SubmitPlayerBid_ValidInput_PassesTurnToMonster()
        {
            Begin();
            yield return WaitFrames(2);
            var raise = new Bid(Match.CurrentBid.Value.Quantity + 1, Match.CurrentBid.Value.Face);

            var result = _controller.SubmitPlayerBid(raise.Quantity.ToString(), raise.Face.ToString());

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(string.Empty, View.ErrorMessage);
            Assert.AreEqual(raise, Match.CurrentBid);
            Assert.AreEqual(Side.Monster, Match.CurrentTurn);
            Assert.AreEqual("你喊：" + raise, View.CurrentBidLabel);

            yield return WaitFrames(2);

            Assert.IsTrue(_controller.IsPlayerTurn || View.IsResultVisible);
        }

        [UnityTest]
        public IEnumerator ChallengeAsPlayer_ShowsResultAndCostsLoserOxygen()
        {
            Begin();
            yield return WaitFrames(2);

            _controller.ChallengeAsPlayer();

            Assert.AreEqual(MatchPhase.RoundOver, Match.Phase);
            Assert.IsTrue(View.IsResultVisible);
            Assert.AreEqual(3, Match.PlayerOxygen + Match.MonsterOxygen);
            StringAssert.StartsWith("你質疑", View.ResultMessage);
        }

        [UnityTest]
        public IEnumerator Continue_AfterRound_StartsNextRoundWithLoserFirst()
        {
            Begin();
            yield return WaitFrames(2);
            _controller.ChallengeAsPlayer();
            var loser = Match.LastResult.Value.Loser;

            _controller.Continue();

            Assert.IsFalse(View.IsResultVisible);
            Assert.AreEqual(MatchPhase.Bidding, Match.Phase);
            Assert.AreEqual(loser, Match.CurrentTurn);
        }

        [UnityTest]
        public IEnumerator LastOxygenLost_RaisesMatchFinishedAndOffersRematch()
        {
            Side? winner = null;
            _controller.MatchFinished += side => winner = side;
            Begin(1);
            yield return WaitFrames(2);

            _controller.ChallengeAsPlayer();

            Assert.AreEqual(MatchPhase.MatchOver, Match.Phase);
            Assert.AreEqual(Match.Winner, winner);
            StringAssert.Contains(LiarDiceText.MatchOver(winner.Value), View.ResultMessage);

            _controller.Continue();

            Assert.AreEqual(MatchPhase.Bidding, Match.Phase);
            Assert.AreEqual(1, Match.PlayerOxygen);
            Assert.AreEqual(1, Match.MonsterOxygen);
        }

        [Test]
        public void SubmitPlayerBid_DuringMonsterTurn_IsRejected()
        {
            Begin();

            var result = _controller.SubmitPlayerBid("3", "4");

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(LiarDiceText.NotPlayerTurn, View.ErrorMessage);
        }
    }
}
