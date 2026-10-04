using System.Collections.Generic;
using FGJ.Exploration;
using FGJ.Flow;
using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using NUnit.Framework;
using UnityEngine;

namespace FGJ.Tests.EditMode.Exploration
{
    public class GameProgressTests
    {
        private readonly List<Object> _created = new List<Object>();
        private GameProgress _progress;

        [SetUp]
        public void SetUp()
        {
            _progress = Create<GameProgress>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var created in _created)
                Object.DestroyImmediate(created);
            _created.Clear();
        }

        private T Create<T>() where T : ScriptableObject
        {
            var instance = ScriptableObject.CreateInstance<T>();
            _created.Add(instance);
            return instance;
        }

        private BuildingDefinition CreateBuilding(string id, LiarDiceConfig config = null)
        {
            var building = Create<BuildingDefinition>();
            building.Configure(id, "enter", "cleared", 1.5f, config);
            return building;
        }

        [Test]
        public void CompleteBuilding_PlayerWon_MarksClearedAndSetsReturnPoint()
        {
            _progress.EnterBuilding(CreateBuilding("Room01"));

            _progress.CompleteBuilding(true);

            Assert.IsTrue(_progress.IsCleared("Room01"));
            Assert.IsNull(_progress.CurrentBuilding);
            Assert.AreEqual("Room01", _progress.ConsumeReturnBuilding());
            Assert.IsNull(_progress.ConsumeReturnBuilding());
        }

        [Test]
        public void CompleteBuilding_PlayerWon_RemembersEntranceXForReturn()
        {
            _progress.EnterBuilding(CreateBuilding("Room01"), 164f);

            _progress.CompleteBuilding(true);

            Assert.AreEqual(164f, _progress.ReturnX, 1e-4f);
        }

        [Test]
        public void CompleteBuilding_PlayerLost_KeepsBuildingForRetry()
        {
            _progress.EnterBuilding(CreateBuilding("Room01"));
            _progress.CompleteBuilding(true, 3);
            var second = CreateBuilding("Room02");
            _progress.EnterBuilding(second);

            _progress.CompleteBuilding(false);

            Assert.IsTrue(_progress.IsCleared("Room01"));
            Assert.IsFalse(_progress.IsCleared("Room02"));
            Assert.AreSame(second, _progress.CurrentBuilding);
            Assert.AreEqual(3, _progress.PlayerOxygenFor(5));
        }

        [Test]
        public void PlayerOxygenFor_FreshRun_StartsAtMaximum()
        {
            Assert.AreEqual(5, _progress.PlayerOxygenFor(5));
        }

        [Test]
        public void CompleteBuilding_PlayerWon_CarriesOxygenIntoNextBuilding()
        {
            _progress.EnterBuilding(CreateBuilding("Room01"));

            _progress.CompleteBuilding(true, 4);

            Assert.AreEqual(4, _progress.PlayerOxygenFor(5));
            Assert.AreEqual(3, _progress.PlayerOxygenFor(3));
        }

        [Test]
        public void CompleteBuilding_WithoutEnteringBuilding_DoesNotClearAnything()
        {
            _progress.CompleteBuilding(true);

            Assert.IsNull(_progress.ConsumeReturnBuilding());
            Assert.IsFalse(_progress.IsCleared(null));
        }

        [Test]
        public void EnterBuilding_ExposesBuildingConfigUntilCompleted()
        {
            var config = Create<LiarDiceConfig>();
            _progress.EnterBuilding(CreateBuilding("Room01", config));

            Assert.AreSame(config, _progress.CurrentBuilding.GameplayConfig);
        }

        [Test]
        public void ResetRun_ClearsEverything()
        {
            _progress.EnterBuilding(CreateBuilding("Room01"));
            _progress.CompleteBuilding(true, 2);

            _progress.ResetRun();

            Assert.IsFalse(_progress.IsCleared("Room01"));
            Assert.IsNull(_progress.ReturnBuildingId);
            Assert.AreEqual(5, _progress.PlayerOxygenFor(5));
        }

        [TestCase(Side.Player, true, false, ExpectedResult = "繼續探索")]
        [TestCase(Side.Player, true, true, ExpectedResult = "觀看結局")]
        [TestCase(Side.Monster, true, false, ExpectedResult = "重新挑戰")]
        [TestCase(Side.Monster, true, true, ExpectedResult = "重新挑戰")]
        [TestCase(Side.Player, false, false, ExpectedResult = "再挑戰一次")]
        public string MatchOverLabel_DependsOnWinnerRoomExitAndFinalLevel(Side winner, bool leavesRoom, bool endsGame)
        {
            return new LiarDiceText().MatchOverLabel(winner, leavesRoom, endsGame);
        }
    }

    public class SceneRouterTests
    {
        private RecordingSceneRouter _router;

        [SetUp]
        public void SetUp() => _router = ScriptableObject.CreateInstance<RecordingSceneRouter>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_router);

        [Test]
        public void GoToStart_MainMenuAvailable_LoadsMainMenu()
        {
            _router.GoToStart();
            CollectionAssert.AreEqual(new[] { SceneNames.MainMenu }, _router.LoadedScenes);
        }

        [Test]
        public void GoToStart_WithoutMainMenu_FallsBackToExploration()
        {
            _router.Availability = scene => scene != SceneNames.MainMenu;

            _router.GoToStart();

            CollectionAssert.AreEqual(new[] { SceneNames.Exploration }, _router.LoadedScenes);
        }

        [Test]
        public void GoToExplorationAndGameplay_LoadConfiguredScenes()
        {
            _router.GoToGameplay();
            _router.GoToExploration();

            CollectionAssert.AreEqual(new[] { SceneNames.Gameplay, SceneNames.Exploration }, _router.LoadedScenes);
        }

        [Test]
        public void GoToEnding_EndingSceneAvailable_LoadsEnding()
        {
            _router.GoToEnding();
            CollectionAssert.AreEqual(new[] { SceneNames.Ending }, _router.LoadedScenes);
        }

        [Test]
        public void GoToEnding_WithoutEndingScene_FallsBackToStartScene()
        {
            _router.Availability = scene => scene != SceneNames.Ending;

            _router.GoToEnding();

            CollectionAssert.AreEqual(new[] { SceneNames.MainMenu }, _router.LoadedScenes);
        }
    }
}
