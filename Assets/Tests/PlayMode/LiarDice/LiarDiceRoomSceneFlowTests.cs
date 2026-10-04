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
        private static readonly Bid TrueBid = new Bid(5, 4);
        private static readonly Bid FalseBid = new Bid(10, 6);

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
        }

        [TearDown]
        public void TearDown()
        {
            if (_controller != null)
                Object.DestroyImmediate(_controller.gameObject);
            Object.Destroy(_progress);
            Object.Destroy(_router);
            Object.Destroy(_building);
            Object.Destroy(_buildingConfig);
            Object.Destroy(_buildingMonster);
        }

        private void CreateRoom()
        {
            _controller = TestPrefabs.Instantiate<LiarDiceRoomController>(TestPrefabs.LiarDiceRoomPath);
            _controller.SetMonsterThinkSeconds(0f);
            _controller.Table.SetAnimationDurations(0f, 0f);
            var flowObject = new GameObject("Flow");
            flowObject.SetActive(false);
            flowObject.transform.SetParent(_controller.transform);
            flowObject.AddComponent<LiarDiceRoomSceneFlow>().Configure(_controller, _progress, _router);
            flowObject.SetActive(true);
        }

        private void BeginOneOxygenMatch()
        {
            var alwaysChallenges = new MonsterAIProfile(1f, 0.5f, 0f, 2);
            _controller.Begin(new MatchSettings(5, 5, 1, 1, Side.Monster, null, 0, 5),
                () => new CyclingDiceRoller(DiceSequence), new MonsterAI(alwaysChallenges, new System.Random(7)));
        }

        private IEnumerator WaitUntil(System.Func<bool> condition)
        {
            for (var frame = 0; frame < 60 && !condition(); frame++)
                yield return null;
            Assert.IsTrue(condition(), "等待逾時");
        }

        private IEnumerator FinishMatch(bool playerWins)
        {
            CreateRoom();
            BeginOneOxygenMatch();
            yield return WaitUntil(() => _controller.IsPlayerTurn);
            _controller.Believe();
            var bid = playerWins ? TrueBid : FalseBid;
            _controller.SubmitPlayerBid(bid.Quantity.ToString(), bid.Face.ToString());
            yield return WaitUntil(() => _controller.Match.Phase == MatchPhase.MatchOver && !_controller.IsBusy);
            Assert.AreEqual(playerWins ? Side.Player : Side.Monster, _controller.Match.Winner);
        }

        [UnityTest]
        public IEnumerator Awake_UsesConfigAndMonsterOfBuildingBeingEntered()
        {
            CreateRoom();
            yield return WaitUntil(() => _controller.IsPlayerTurn);

            Assert.AreSame(_buildingMonster, _controller.Monster);
            Assert.AreSame(_buildingConfig, _controller.Config);
            Assert.AreEqual("測試怪物：「測試開場」", _controller.Log.Entries[0].Text);
            Assert.AreEqual($"測試怪物：<{_controller.Match.CurrentBid.Value}>", _controller.Hud.LogView.LatestText);

            Assert.AreEqual(_buildingConfig.PlayerOxygen, _controller.Match.PlayerOxygen);
            Assert.AreEqual(_buildingConfig.PlayerDiceCount, _controller.Match.GetDice(Side.Player).Count);
        }

        [UnityTest]
        public IEnumerator Awake_AfterEarlierVictory_StartsWithCarriedOxygen()
        {
            _progress.CompleteBuilding(true, 3);
            _progress.EnterBuilding(_building);

            CreateRoom();
            yield return WaitUntil(() => _controller.IsPlayerTurn);

            Assert.AreEqual(3, _controller.Match.PlayerOxygen);
            Assert.AreEqual(5, _controller.Match.GetMaxOxygen(Side.Player));
            Assert.AreEqual(5, _controller.Hud.PlayerOxygen.SegmentCount);
            Assert.AreEqual(3, _controller.Hud.PlayerOxygen.FilledCount);
        }

        [UnityTest]
        public IEnumerator PlayerWins_ContinuesExploringWithRewardOxygen()
        {
            yield return FinishMatch(true);
            Assert.AreEqual(LiarDiceText.ContinueExploringLabel, _controller.Hud.ContinueLabel);

            _controller.Continue();

            CollectionAssert.AreEqual(new[] { SceneNames.Exploration }, _router.LoadedScenes);
            Assert.IsTrue(_progress.IsCleared("Room01"));
            Assert.AreEqual(3, _progress.PlayerOxygenFor(5));
        }

        [UnityTest]
        public IEnumerator PlayerLoses_RetriesSameBuilding()
        {
            yield return FinishMatch(false);
            Assert.AreEqual(LiarDiceText.RetryLabel, _controller.Hud.ContinueLabel);

            _controller.Continue();

            CollectionAssert.AreEqual(new[] { SceneNames.Gameplay }, _router.LoadedScenes);
            Assert.AreSame(_building, _progress.CurrentBuilding);
            Assert.IsFalse(_progress.IsCleared("Room01"));
        }

        [UnityTest]
        public IEnumerator FinalLevelWin_GoesToEndingAndResetsRun()
        {
            _buildingConfig.Configure(5, 2, 0, true);
            yield return FinishMatch(true);
            Assert.AreEqual(LiarDiceText.EndingLabel, _controller.Hud.ContinueLabel);

            _controller.Continue();

            CollectionAssert.AreEqual(new[] { SceneNames.Ending }, _router.LoadedScenes);
            Assert.IsFalse(_progress.IsCleared("Room01"));
            Assert.IsNull(_progress.CurrentBuilding);
        }

        [UnityTest]
        public IEnumerator FinalLevelWinWithoutEndingScene_FallsBackToStart()
        {
            _router.Availability = scene => scene != SceneNames.Ending;
            _buildingConfig.Configure(5, 2, 0, true);
            yield return FinishMatch(true);

            _controller.Continue();

            CollectionAssert.AreEqual(new[] { SceneNames.MainMenu }, _router.LoadedScenes);
        }
    }
}
