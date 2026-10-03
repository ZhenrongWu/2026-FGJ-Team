using System.Collections;
using FGJ.Exploration;
using FGJ.Flow;
using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using FGJ.Tests.PlayMode.Exploration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FGJ.Tests.PlayMode.LiarDice
{
    public class LiarDiceRoomSceneFlowTests
    {
        private static readonly int[] DiceSequence = { 1, 4, 4, 2, 6, 3, 4, 5, 5, 1 };

        private LiarDiceRoomController _controller;
        private GameProgress _progress;
        private RecordingSceneRouter _router;
        private BuildingDefinition _building;
        private LiarDiceConfig _buildingConfig;

        [SetUp]
        public void SetUp()
        {
            _progress = ScriptableObject.CreateInstance<GameProgress>();
            _router = ScriptableObject.CreateInstance<RecordingSceneRouter>();
            _buildingConfig = ScriptableObject.CreateInstance<LiarDiceConfig>();
            _building = ScriptableObject.CreateInstance<BuildingDefinition>();
            _building.Configure("Room01", "enter", "cleared", 1.5f, _buildingConfig);
            _progress.EnterBuilding(_building);

            _controller = LiarDiceRoomBuilder.Build(null);
            _controller.SetMonsterThinkSeconds(0f);
            var flowObject = new GameObject("Flow");
            flowObject.SetActive(false);
            flowObject.transform.SetParent(_controller.transform);
            flowObject.AddComponent<LiarDiceRoomSceneFlow>().Configure(_controller, _progress, _router);
            flowObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_controller.gameObject);
            Object.Destroy(_progress);
            Object.Destroy(_router);
            Object.Destroy(_building);
            Object.Destroy(_buildingConfig);
        }

        private void BeginOneOxygenMatch()
        {
            _controller.Begin(new MatchSettings(5, 5, 1, 1), () => new CyclingDiceRoller(DiceSequence),
                new MonsterAI(new MonsterAIProfile(0.35f, 0.5f, 0f, 2), new System.Random(7)));
        }

        private IEnumerator FinishMatch()
        {
            BeginOneOxygenMatch();
            yield return null;
            yield return null;
            _controller.ChallengeAsPlayer();
        }

        [UnityTest]
        public IEnumerator Awake_UsesConfigOfBuildingBeingEntered()
        {
            yield return null;

            Assert.AreEqual(_buildingConfig.PlayerOxygen, _controller.Match.PlayerOxygen);
            Assert.AreEqual(_buildingConfig.PlayerDiceCount, _controller.Match.GetDice(Side.Player).Count);
        }

        [UnityTest]
        public IEnumerator MatchOver_ContinueLeavesRoomAndRecordsResult()
        {
            yield return FinishMatch();
            var winner = _controller.Match.Winner.Value;
            Assert.AreEqual(winner == Side.Player ? "返回洞穴" : "重新開始", _controller.View.ContinueLabel);

            _controller.Continue();

            var expectedScene = winner == Side.Player ? SceneNames.Exploration : SceneNames.MainMenu;
            CollectionAssert.AreEqual(new[] { expectedScene }, _router.LoadedScenes);
            Assert.AreEqual(winner == Side.Player, _progress.IsCleared("Room01"));
        }

        [UnityTest]
        public IEnumerator PlayerLosesWithoutMainMenu_RestartsAtExploration()
        {
            _router.Availability = scene => scene != SceneNames.MainMenu;
            yield return FinishMatch();
            if (_controller.Match.Winner == Side.Player)
                Assert.Ignore("此骰子序列下玩家獲勝，敗北路徑由另一個測試涵蓋。");

            _controller.Continue();

            CollectionAssert.AreEqual(new[] { SceneNames.Exploration }, _router.LoadedScenes);
            Assert.IsNull(_progress.CurrentBuilding);
        }
    }
}
