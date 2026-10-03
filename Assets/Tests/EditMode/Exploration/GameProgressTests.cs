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
        public void CompleteBuilding_PlayerLost_ResetsWholeRun()
        {
            _progress.EnterBuilding(CreateBuilding("Room01"));
            _progress.CompleteBuilding(true);
            _progress.EnterBuilding(CreateBuilding("Room02"));

            _progress.CompleteBuilding(false);

            Assert.IsFalse(_progress.IsCleared("Room01"));
            Assert.IsNull(_progress.CurrentBuilding);
            Assert.IsNull(_progress.ConsumeReturnBuilding());
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
            _progress.CompleteBuilding(true);

            _progress.ResetRun();

            Assert.IsFalse(_progress.IsCleared("Room01"));
            Assert.IsNull(_progress.ReturnBuildingId);
        }

        [TestCase(Side.Player, true, ExpectedResult = "返回洞穴")]
        [TestCase(Side.Monster, true, ExpectedResult = "重新開始")]
        [TestCase(Side.Player, false, ExpectedResult = "再挑戰一次")]
        public string MatchOverLabel_DependsOnWinnerAndRoomExit(Side winner, bool leavesRoom)
        {
            return new LiarDiceText().MatchOverLabel(winner, leavesRoom);
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
    }
}
