using System.Collections;
using FGJ.Exploration;
using FGJ.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FGJ.Tests.PlayMode.Exploration
{
    public class ExplorationControllerTests
    {
        private ExplorationTestRig _rig;

        [SetUp]
        public void SetUp()
        {
            _rig = new ExplorationTestRig();
        }

        [TearDown]
        public void TearDown()
        {
            _rig.Dispose();
        }

        private static IEnumerator WaitFrames(int count)
        {
            for (var i = 0; i < count; i++)
                yield return null;
        }

        [UnityTest]
        public IEnumerator Start_SpawnsBuildingsFromRoute()
        {
            _rig.Build(2f);
            yield return null;

            var entrances = _rig.Controller.Entrances;
            Assert.AreEqual(1, entrances.Count);
            Assert.AreEqual(ExplorationTestRig.RoomId, entrances[0].RoomId);
            Assert.AreEqual(ExplorationTestRig.BuildingX, entrances[0].X, 1e-4f);
            Assert.IsNotNull(entrances[0].transform.Find("PlaceholderMarker"));
        }

        [UnityTest]
        public IEnumerator HoldingRight_MovesPlayerAndPlaysWalk()
        {
            _rig.Build(2f);
            yield return null;
            var player = _rig.Player;
            var startX = player.X;
            var animator = player.GetComponent<SpriteFrameAnimator>();

            player.SetMoveInputOverride(1f);
            yield return WaitFrames(3);

            Assert.Greater(player.X, startX);
            Assert.AreEqual(SideScrollPlayer.WalkClip, animator.CurrentClipName);
            Assert.IsFalse(player.GetComponent<SpriteRenderer>().flipX);

            player.SetMoveInputOverride(-1f);
            yield return WaitFrames(2);
            Assert.IsTrue(player.GetComponent<SpriteRenderer>().flipX);

            player.SetMoveInputOverride(0f);
            yield return null;
            Assert.AreEqual(SideScrollPlayer.IdleClip, animator.CurrentClipName);
        }

        [UnityTest]
        public IEnumerator FarFromBuilding_HidesPrompt()
        {
            _rig.Build(2f);
            yield return WaitFrames(2);

            Assert.IsNull(_rig.Controller.NearbyEntrance);
            Assert.AreEqual(string.Empty, _rig.Hud.PromptMessage);
        }

        [UnityTest]
        public IEnumerator NearBuilding_InteractEntersGameplayWithBuildingConfig()
        {
            _rig.Build(ExplorationTestRig.BuildingX + 1f);
            yield return WaitFrames(2);
            Assert.AreEqual(ExplorationTestRig.EnterPrompt, _rig.Hud.PromptMessage);

            _rig.Controller.RequestInteract();
            yield return WaitFrames(3);

            Assert.IsTrue(_rig.Controller.IsEnteringRoom);
            Assert.IsTrue(_rig.Player.InputLocked);
            Assert.AreEqual(SideScrollPlayer.EnterClip,
                _rig.Player.GetComponent<SpriteFrameAnimator>().CurrentClipName);
            Assert.AreSame(_rig.Building, _rig.Progress.CurrentBuilding);
            Assert.AreSame(_rig.BuildingConfig, _rig.Progress.CurrentBuilding.GameplayConfig);
            CollectionAssert.AreEqual(new[] { SceneNames.Gameplay }, _rig.Router.LoadedScenes);
        }

        [UnityTest]
        public IEnumerator ReturningFromClearedBuilding_SpawnsThereAndBlocksReentry()
        {
            _rig.Progress.EnterBuilding(_rig.Building);
            _rig.Progress.CompleteBuilding(true);
            _rig.Build(2f);
            yield return WaitFrames(2);

            Assert.AreEqual(ExplorationTestRig.BuildingX, _rig.Player.X, 1e-4f);
            Assert.AreEqual(ExplorationTestRig.ClearedPrompt, _rig.Hud.PromptMessage);
            Assert.IsFalse(_rig.Controller.Entrances[0].transform.Find("PlaceholderMarker").gameObject.activeSelf);

            _rig.Controller.RequestInteract();
            yield return WaitFrames(2);

            Assert.IsFalse(_rig.Controller.IsEnteringRoom);
            Assert.IsEmpty(_rig.Router.LoadedScenes);
        }
    }
}
