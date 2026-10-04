using System;
using System.Collections;
using FGJ.Audio;
using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace FGJ.Tests.PlayMode.LiarDice
{
    public class LiarDiceRoomTests
    {
        private const int FrameLimit = 60;
        private static readonly int[] DiceSequence = { 1, 4, 4, 2, 6, 3, 4, 5, 5, 1 };
        private static readonly int[] SixesAndTwos = { 6, 6, 6, 6, 6, 2, 2, 2, 2, 2 };

        private LiarDiceRoomController _controller;
        private RecordingGameAudio _audio;

        private LiarDiceHud Hud => _controller.Hud;
        private LiarDiceMatch Match => _controller.Match;

        [SetUp]
        public void SetUp()
        {
            _controller = TestPrefabs.Instantiate<LiarDiceRoomController>(TestPrefabs.LiarDiceRoomPath);
            _audio = RecordingGameAudio.Create();
            _controller.SetAudio(_audio);
            _controller.Hud.SetAudio(_audio);
            _controller.SetMonsterThinkSeconds(0f);
            _controller.Table.SetAnimationDurations(0f, 0f);
            _controller.Table.SetYawRandom(new System.Random(3));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_controller.gameObject);
            Object.Destroy(_audio);
        }

        private void Begin(int oxygen = 2)
        {
            var profile = new MonsterAIProfile(0.35f, 0.5f, 0f, 2);
            _controller.Begin(new MatchSettings(5, 5, oxygen, oxygen), () => new CyclingDiceRoller(DiceSequence),
                new MonsterAI(profile, new System.Random(7)));
        }

        private static IEnumerator WaitUntil(Func<bool> condition)
        {
            for (var frame = 0; frame < FrameLimit && !condition(); frame++)
                yield return null;
            Assert.IsTrue(condition(), "等待逾時");
        }

        private IEnumerator WaitForPlayerTurn() => WaitUntil(() => _controller.IsPlayerTurn);

        private IEnumerator WaitForIdle() => WaitUntil(() => !_controller.IsBusy);

        [UnityTest]
        public IEnumerator Begin_MonsterOpensRoundThenPlayerCanBelieveOrBluff()
        {
            Begin();
            Assert.AreEqual(Side.Monster, Match.CurrentTurn);
            Assert.IsFalse(Hud.IsBelieveEnabled);

            yield return WaitForPlayerTurn();

            Assert.IsTrue(Match.CurrentBid.HasValue);
            Assert.IsTrue(Hud.IsBelieveEnabled);
            Assert.IsTrue(Hud.IsBluffEnabled);
            Assert.IsFalse(Hud.IsBidInputEnabled);
            StringAssert.StartsWith("怪物喊：", Hud.StatusMessage);
            Assert.AreEqual($"{_controller.Monster.DisplayName}：{_controller.Monster.BidLine(Match.CurrentBid.Value)}",
                Hud.LogView.LatestText);
            Assert.AreEqual(2, Hud.PlayerOxygen.FilledCount);
            Assert.IsNotNull(_controller.MonsterView.CurrentSprite);
            Assert.AreEqual("BlobfishScumbag", _controller.MonsterView.CurrentSprite.name);
            Assert.IsFalse(_controller.MonsterView.IsShowingPlaceholder);
            Assert.AreEqual(2, Hud.MonsterOxygen.SegmentCount);
        }

        [UnityTest]
        public IEnumerator LevelWithoutItems_HidesBothItemBars()
        {
            Begin();
            yield return WaitForPlayerTurn();

            Assert.IsFalse(Hud.PlayerItems.gameObject.activeSelf);
            Assert.IsFalse(Hud.MonsterItems.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator Believe_UnlocksBidInput()
        {
            Begin();
            yield return WaitForPlayerTurn();

            _controller.Believe();

            Assert.IsTrue(_controller.HasBelieved);
            Assert.IsTrue(Hud.IsBidInputEnabled);
            Assert.IsFalse(Hud.IsBelieveEnabled);
            Assert.IsTrue(Hud.IsBluffEnabled);
        }

        [UnityTest]
        public IEnumerator SubmitPlayerBid_InvalidInput_ShowsErrorAndKeepsTurn()
        {
            Begin();
            yield return WaitForPlayerTurn();
            var monsterBid = Match.CurrentBid;
            _controller.Believe();

            var result = _controller.SubmitPlayerBid("abc", "4");

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("數量必須是整數。", Hud.ErrorMessage);
            Assert.AreEqual(monsterBid, Match.CurrentBid);
            Assert.IsTrue(_controller.IsPlayerTurn);
        }

        [UnityTest]
        public IEnumerator SubmitPlayerBid_ValidInput_LogsAndPassesTurnToMonster()
        {
            Begin();
            yield return WaitForPlayerTurn();
            var raise = new Bid(Match.CurrentBid.Value.Quantity + 1, Match.CurrentBid.Value.Face);
            _controller.Believe();

            var result = _controller.SubmitPlayerBid(raise.Quantity.ToString(), raise.Face.ToString());

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(string.Empty, Hud.ErrorMessage);
            Assert.AreEqual(raise, Match.CurrentBid);
            Assert.AreEqual(Side.Monster, Match.CurrentTurn);
            Assert.IsFalse(_controller.HasBelieved);
            Assert.AreEqual($"你相信，喊：{raise}", Hud.LogView.LatestText);

            yield return WaitUntil(() => _controller.IsPlayerTurn || Match.Phase != MatchPhase.Bidding);
        }

        [UnityTest]
        public IEnumerator ChallengeAsPlayer_RevealsMonsterCupAndCostsLoserOxygen()
        {
            Begin();
            yield return WaitForPlayerTurn();

            _controller.ChallengeAsPlayer();
            yield return WaitForIdle();

            Assert.AreEqual(MatchPhase.RoundOver, Match.Phase);
            Assert.IsTrue(_controller.Table.MonsterCup.IsLifted);
            Assert.IsTrue(Hud.IsContinueVisible);
            Assert.AreEqual(LiarDiceText.NextRoundLabel, Hud.ContinueLabel);
            Assert.AreEqual(3, Match.PlayerOxygen + Match.MonsterOxygen);
            Assert.AreEqual(Match.PlayerOxygen, Hud.PlayerOxygen.FilledCount);
            Assert.AreEqual(Match.MonsterOxygen, Hud.MonsterOxygen.FilledCount);
            StringAssert.StartsWith("你喊吹牛", Hud.LogView.LatestText);
        }

        [UnityTest]
        public IEnumerator Begin_PlaysLevelMusicAndShakesDice()
        {
            _audio.Music.Clear();
            _audio.Effects.Clear();

            Begin();
            yield return WaitForPlayerTurn();

            Assert.IsNotNull(_controller.Config.Music);
            CollectionAssert.AreEqual(new[] { _controller.Config.Music }, _audio.Music);
            CollectionAssert.AreEqual(new[] { SoundEffect.DiceShake }, _audio.Effects);
        }

        [UnityTest]
        public IEnumerator Challenge_PlaysOxygenLossOnlyWhenPlayerLoses()
        {
            Begin();
            yield return WaitForPlayerTurn();
            _audio.Effects.Clear();

            _controller.ChallengeAsPlayer();
            yield return WaitForIdle();

            var playerLost = Match.LastResult.Value.Loser == Side.Player;
            CollectionAssert.AreEqual(playerLost ? new[] { SoundEffect.OxygenLoss } : new SoundEffect[0], _audio.Effects);
        }

        [UnityTest]
        public IEnumerator HudButton_Clicked_PlaysButtonPress()
        {
            Begin();
            yield return WaitForPlayerTurn();
            _audio.Effects.Clear();

            Hud.Parts.bluffButton.onClick.Invoke();

            Assert.AreEqual(SoundEffect.ButtonPress, _audio.Effects[0]);
        }

        [UnityTest]
        public IEnumerator Continue_AfterRound_ClosesCupsAndMonsterBidsFirst()
        {
            Begin();
            yield return WaitForPlayerTurn();
            _controller.ChallengeAsPlayer();
            yield return WaitForIdle();

            _controller.Continue();

            Assert.AreEqual(MatchPhase.Bidding, Match.Phase);
            Assert.AreEqual(Side.Monster, Match.CurrentTurn);
            Assert.IsFalse(Hud.IsContinueVisible);
            Assert.IsFalse(_controller.Table.MonsterCup.IsLifted);
            StringAssert.Contains("第 2 局", Hud.LogView.LatestText);
        }

        [UnityTest]
        public IEnumerator LastOxygenLost_RaisesMatchFinishedAndOffersRematch()
        {
            Side? winner = null;
            _controller.MatchFinished += side => winner = side;
            Begin(1);
            yield return WaitForPlayerTurn();

            _controller.ChallengeAsPlayer();
            yield return WaitForIdle();

            Assert.AreEqual(MatchPhase.MatchOver, Match.Phase);
            Assert.AreEqual(Match.Winner, winner);
            Assert.AreEqual(new LiarDiceText().MatchOver(winner.Value), Hud.LogView.LatestText);
            Assert.AreEqual(LiarDiceText.RematchLabel, Hud.ContinueLabel);

            _controller.Continue();

            Assert.AreEqual(MatchPhase.Bidding, Match.Phase);
            Assert.AreEqual(1, Match.PlayerOxygen);
            Assert.AreEqual(1, Match.MonsterOxygen);
        }

        [UnityTest]
        public IEnumerator PlayerWinsMatch_MonsterShowsDeadSpriteUntilRematch()
        {
            var profile = new MonsterAIProfile(0.35f, 0.5f, 0f, 2);
            _controller.Begin(new MatchSettings(5, 5, 5, 1), () => new CyclingDiceRoller(SixesAndTwos),
                new MonsterAI(profile, new System.Random(7)));

            while (Match.Phase != MatchPhase.MatchOver)
            {
                yield return WaitUntil(() => _controller.IsPlayerTurn || Match.Phase != MatchPhase.Bidding);
                if (_controller.IsPlayerTurn)
                    PlayHonestly();
                yield return WaitForIdle();
                if (Match.Phase == MatchPhase.RoundOver)
                    _controller.Continue();
            }
            yield return WaitUntil(() => !_controller.IsBusy && Hud.IsContinueVisible);

            Assert.AreEqual(Side.Player, Match.Winner);
            Assert.IsTrue(_controller.MonsterView.IsShowingDead);
            Assert.AreSame(_controller.Monster.DeadSprite, _controller.MonsterView.CurrentSprite);

            _controller.Continue();

            Assert.IsFalse(_controller.MonsterView.IsShowingDead);
            Assert.AreSame(_controller.Monster.Sprite, _controller.MonsterView.CurrentSprite);
        }

        private void PlayHonestly()
        {
            var bid = Match.CurrentBid;
            if (Match.CanChallenge && bid.HasValue && CountOnTable(bid.Value.Face) < bid.Value.Quantity)
            {
                _controller.ChallengeAsPlayer();
                return;
            }
            if (!_controller.SubmitPlayerBid("5", "6").IsValid && Match.CanChallenge)
                _controller.ChallengeAsPlayer();
        }

        private int CountOnTable(int face)
        {
            var count = 0;
            foreach (var side in new[] { Side.Player, Side.Monster })
            {
                foreach (var die in Match.GetDice(side))
                {
                    if (die == face)
                        count++;
                }
            }
            return count;
        }

        [Test]
        public void SubmitPlayerBid_DuringRoundStart_IsRejected()
        {
            Begin();

            var result = _controller.SubmitPlayerBid("3", "4");

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(LiarDiceText.NotPlayerTurn, Hud.ErrorMessage);
        }
    }
}
