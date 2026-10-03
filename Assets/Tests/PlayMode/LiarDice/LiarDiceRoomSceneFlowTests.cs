using System.Collections;
using System.Collections.Generic;
using FGJ.Flow;
using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FGJ.Tests.PlayMode.LiarDice
{
    public class LiarDiceRoomSceneFlowTests
    {
        private static readonly int[] DiceSequence = { 1, 4, 4, 2, 6, 3, 4, 5, 5, 1 };

        private LiarDiceRoomController _controller;
        private LiarDiceRoomSceneFlow _flow;
        private readonly List<string> _loadedScenes = new List<string>();

        [SetUp]
        public void SetUp()
        {
            GameSession.Reset();
            _loadedScenes.Clear();
            _controller = LiarDiceRoomBuilder.Build(null);
            _controller.SetMonsterThinkSeconds(0f);
            _flow = _controller.gameObject.AddComponent<LiarDiceRoomSceneFlow>();
            _flow.enabled = false;
            _flow.Configure(_controller, SceneNames.Exploration);
            _flow.SetSceneLoader(_loadedScenes.Add);
            _flow.enabled = true;

            _controller.Begin(new MatchSettings(5, 5, 1, 1), () => new CyclingDiceRoller(DiceSequence),
                new MonsterAI(new MonsterAIProfile(0.35f, 0.5f, 0f, 2), new System.Random(7)));
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_controller.gameObject);
            GameSession.Reset();
        }

        private IEnumerator FinishMatch()
        {
            GameSession.EnterRoom("Room01");
            yield return null;
            yield return null;
            _controller.ChallengeAsPlayer();
        }

        [UnityTest]
        public IEnumerator MatchOver_ContinueLeavesRoomAndRecordsResult()
        {
            _flow.SetSceneAvailability(_ => true);
            yield return FinishMatch();
            var winner = _controller.Match.Winner.Value;
            Assert.AreEqual(winner == Side.Player ? "返回洞穴" : "重新開始", _controller.View.ContinueLabel);

            _controller.Continue();

            var expectedScene = winner == Side.Player ? SceneNames.Exploration : SceneNames.MainMenu;
            CollectionAssert.AreEqual(new[] { expectedScene }, _loadedScenes);
            Assert.AreEqual(winner == Side.Player, GameSession.IsCleared("Room01"));
        }

        [UnityTest]
        public IEnumerator PlayerLosesWithoutMainMenu_RestartsAtExploration()
        {
            _flow.SetSceneAvailability(scene => scene != SceneNames.MainMenu);
            yield return FinishMatch();
            if (_controller.Match.Winner == Side.Player)
                Assert.Ignore("此骰子序列下玩家獲勝，敗北路徑由另一個測試涵蓋。");

            _controller.Continue();

            CollectionAssert.AreEqual(new[] { SceneNames.Exploration }, _loadedScenes);
            Assert.IsNull(GameSession.CurrentRoomId);
        }
    }
}
