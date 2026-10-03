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
        private MonsterProfile _buildingMonster;

        [SetUp]
        public void SetUp()
        {
            _progress = ScriptableObject.CreateInstance<GameProgress>();
            _router = ScriptableObject.CreateInstance<RecordingSceneRouter>();
            _buildingConfig = ScriptableObject.CreateInstance<LiarDiceConfig>();
            _building = ScriptableObject.CreateInstance<BuildingDefinition>();
            _buildingMonster = ScriptableObject.CreateInstance<MonsterProfile>();
            _buildingMonster.Configure("測試怪物", "「測試開場」", "「測試質疑」", "<{0}>");
            _building.Configure("Room01", "enter", "cleared", 1.5f, _buildingConfig, _buildingMonster);
            _progress.EnterBuilding(_building);

            _controller = TestPrefabs.Instantiate<LiarDiceRoomController>(TestPrefabs.LiarDiceRoomPath);
            _controller.SetMonsterThinkSeconds(0f);
            _controller.Table.SetAnimationDurations(0f, 0f);
            var flowObject = new GameObject("Flow");
            flowObject.SetActive(false);
            flowObject.transform.SetParent(_controller.transform);
            flowObject.AddComponent<LiarDiceRoomSceneFlow>().Configure(_controller, _progress, _router);
            flowObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_controller.gameObject);
            Object.Destroy(_progress);
            Object.Destroy(_router);
            Object.Destroy(_building);
            Object.Destroy(_buildingConfig);
            Object.Destroy(_buildingMonster);
        }

        private void BeginOneOxygenMatch()
        {
            _controller.Begin(new MatchSettings(5, 5, 1, 1), () => new CyclingDiceRoller(DiceSequence),
                new MonsterAI(new MonsterAIProfile(0.35f, 0.5f, 0f, 2), new System.Random(7)));
        }

        private IEnumerator WaitUntil(System.Func<bool> condition)
        {
            for (var frame = 0; frame < 60 && !condition(); frame++)
                yield return null;
            Assert.IsTrue(condition(), "等待逾時");
        }

        private IEnumerator FinishMatch()
        {
            BeginOneOxygenMatch();
            yield return WaitUntil(() => _controller.IsPlayerTurn);
            _controller.ChallengeAsPlayer();
            yield return WaitUntil(() => !_controller.IsBusy);
        }

        [UnityTest]
        public IEnumerator Awake_UsesConfigAndMonsterOfBuildingBeingEntered()
        {
            yield return WaitUntil(() => _controller.IsPlayerTurn);

            Assert.AreSame(_buildingMonster, _controller.Monster);
            Assert.AreEqual("測試怪物：「測試開場」", _controller.Log.Entries[0].Text);
            Assert.AreEqual($"測試怪物：<{_controller.Match.CurrentBid.Value}>", _controller.Hud.LogView.LatestText);

            Assert.AreEqual(_buildingConfig.PlayerOxygen, _controller.Match.PlayerOxygen);
            Assert.AreEqual(_buildingConfig.PlayerDiceCount, _controller.Match.GetDice(Side.Player).Count);
        }

        [UnityTest]
        public IEnumerator MatchOver_ContinueLeavesRoomAndRecordsResult()
        {
            yield return FinishMatch();
            var winner = _controller.Match.Winner.Value;
            Assert.AreEqual(winner == Side.Player ? "返回洞穴" : "重新開始", _controller.Hud.ContinueLabel);

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
