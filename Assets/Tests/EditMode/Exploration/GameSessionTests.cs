using FGJ.Flow;
using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using NUnit.Framework;

namespace FGJ.Tests.EditMode.Exploration
{
    public class GameSessionTests
    {
        [SetUp]
        public void SetUp() => GameSession.Reset();

        [TearDown]
        public void TearDown() => GameSession.Reset();

        [Test]
        public void CompleteRoom_PlayerWon_MarksClearedAndSetsReturnPoint()
        {
            GameSession.EnterRoom("Room01");

            GameSession.CompleteRoom(true);

            Assert.IsTrue(GameSession.IsCleared("Room01"));
            Assert.IsNull(GameSession.CurrentRoomId);
            Assert.AreEqual("Room01", GameSession.ConsumeReturnRoom());
            Assert.IsNull(GameSession.ConsumeReturnRoom());
        }

        [Test]
        public void CompleteRoom_PlayerLost_ResetsWholeRun()
        {
            GameSession.EnterRoom("Room01");
            GameSession.CompleteRoom(true);
            GameSession.EnterRoom("Room02");

            GameSession.CompleteRoom(false);

            Assert.IsFalse(GameSession.IsCleared("Room01"));
            Assert.IsNull(GameSession.ConsumeReturnRoom());
        }

        [Test]
        public void CompleteRoom_WithoutEnteringRoom_DoesNotClearAnything()
        {
            GameSession.CompleteRoom(true);

            Assert.IsNull(GameSession.ConsumeReturnRoom());
            Assert.IsFalse(GameSession.IsCleared(null));
        }

        [Test]
        public void EnterRoom_KeepsGameplayConfigUntilRoomCompletes()
        {
            var config = UnityEngine.ScriptableObject.CreateInstance<LiarDiceConfig>();
            try
            {
                GameSession.EnterRoom("Room01", config);
                Assert.AreSame(config, GameSession.CurrentGameplayConfig);

                GameSession.CompleteRoom(true);
                Assert.IsNull(GameSession.CurrentGameplayConfig);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void StartScene_PrefersMainMenuWhenAvailable()
        {
            Assert.AreEqual(SceneNames.MainMenu, SceneFlow.StartScene(_ => true));
        }

        [Test]
        public void StartScene_WithoutMainMenu_FallsBackToExploration()
        {
            Assert.AreEqual(SceneNames.Exploration, SceneFlow.StartScene(scene => scene != SceneNames.MainMenu));
            Assert.AreEqual(SceneNames.Exploration, SceneFlow.StartScene(_ => false));
        }

        [TestCase(Side.Player, true, ExpectedResult = "返回洞穴")]
        [TestCase(Side.Monster, true, ExpectedResult = "重新開始")]
        [TestCase(Side.Player, false, ExpectedResult = "再挑戰一次")]
        public string MatchOverLabel_DependsOnWinnerAndRoomExit(Side winner, bool leavesRoom)
        {
            return LiarDiceText.MatchOverLabel(winner, leavesRoom);
        }
    }
}
