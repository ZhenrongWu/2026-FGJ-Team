using System.Collections;
using FGJ.Audio;
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
            Assert.IsFalse(entrances[0].IsCleared);
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
        public IEnumerator Start_PlaysExplorationMusic()
        {
            _rig.Build(2f);
            yield return null;

            CollectionAssert.AreEqual(new[] { _rig.Audio.ExplorationMusic }, _rig.Audio.Music);
        }

        [UnityTest]
        public IEnumerator Walking_LoopsFootstepsUntilPlayerStops()
        {
            _rig.Build(2f);
            yield return null;

            _rig.Player.SetMoveInputOverride(1f);
            yield return WaitFrames(3);
            CollectionAssert.AreEqual(new[] { true }, _rig.Audio.FootstepChanges);

            _rig.Player.SetMoveInputOverride(0f);
            yield return WaitFrames(2);
            CollectionAssert.AreEqual(new[] { true, false }, _rig.Audio.FootstepChanges);
        }

        [UnityTest]
        public IEnumerator EnteringWhileWalking_StopsFootsteps()
        {
            _rig.Build(2f);
            yield return null;
            _rig.Player.SetMoveInputOverride(1f);
            yield return WaitFrames(2);

            _rig.Player.PlayEnter();

            CollectionAssert.AreEqual(new[] { true, false }, _rig.Audio.FootstepChanges);
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
            CollectionAssert.AreEqual(new[] { SoundEffect.DoorOpen }, _rig.Audio.Effects);
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
            Assert.IsTrue(_rig.Controller.Entrances[0].IsCleared);

            _rig.Controller.RequestInteract();
            yield return WaitFrames(2);

            Assert.IsFalse(_rig.Controller.IsEnteringRoom);
            Assert.IsEmpty(_rig.Router.LoadedScenes);
        }

        [UnityTest]
        public IEnumerator WalkingPastLastBuilding_BuildingRepeatsInNextLoop()
        {
            _rig.LoopLength = 20f;
            _rig.Build(ExplorationTestRig.BuildingX + 21f);
            yield return WaitFrames(2);

            Assert.AreEqual(ExplorationTestRig.BuildingX + 20f, _rig.Controller.Entrances[0].X, 1e-4f);
            Assert.AreSame(_rig.Controller.Entrances[0], _rig.Controller.NearbyEntrance);
            Assert.AreEqual(ExplorationTestRig.EnterPrompt, _rig.Hud.PromptMessage);
        }

        [UnityTest]
        public IEnumerator EnteringLoopedBuilding_ReturnsPlayerToSameLoop()
        {
            _rig.LoopLength = 20f;
            _rig.Build(ExplorationTestRig.BuildingX + 41f);
            yield return WaitFrames(2);

            _rig.Controller.RequestInteract();
            yield return WaitFrames(3);
            _rig.Progress.CompleteBuilding(true);

            Assert.AreEqual(ExplorationTestRig.BuildingX + 40f, _rig.Progress.ReturnX, 1e-4f);
        }

        [UnityTest]
        public IEnumerator ReturningFromLoopedBuilding_SpawnsAtThatLoopCopy()
        {
            _rig.LoopLength = 20f;
            _rig.Progress.EnterBuilding(_rig.Building, ExplorationTestRig.BuildingX + 40f);
            _rig.Progress.CompleteBuilding(true);
            _rig.Build(2f);
            yield return WaitFrames(2);

            Assert.AreEqual(ExplorationTestRig.BuildingX + 40f, _rig.Player.X, 1e-4f);
            Assert.AreEqual(ExplorationTestRig.BuildingX + 40f, _rig.Controller.Entrances[0].X, 1e-4f);
            Assert.AreEqual(ExplorationTestRig.ClearedPrompt, _rig.Hud.PromptMessage);
        }

        [UnityTest]
        public IEnumerator NearBuilding_HighlightsItAndLeavingTurnsItOff()
        {
            _rig.Build(ExplorationTestRig.BuildingX + 1f);
            yield return WaitFrames(2);
            Assert.IsTrue(_rig.Controller.Entrances[0].IsHighlighted);

            _rig.Player.PlaceAt(2f);
            yield return WaitFrames(2);

            Assert.IsFalse(_rig.Controller.Entrances[0].IsHighlighted);
        }

        [UnityTest]
        public IEnumerator NearClearedBuilding_DoesNotHighlight()
        {
            _rig.Progress.EnterBuilding(_rig.Building);
            _rig.Progress.CompleteBuilding(true);
            _rig.Build(2f);
            yield return WaitFrames(2);

            Assert.AreSame(_rig.Controller.Entrances[0], _rig.Controller.NearbyEntrance);
            Assert.IsFalse(_rig.Controller.Entrances[0].IsHighlighted);
        }

        [UnityTest]
        public IEnumerator EnteringBuilding_TurnsHighlightOff()
        {
            _rig.Build(ExplorationTestRig.BuildingX + 1f);
            yield return WaitFrames(2);

            _rig.Controller.RequestInteract();
            yield return WaitFrames(2);

            Assert.IsTrue(_rig.Controller.IsEnteringRoom);
            Assert.IsFalse(_rig.Controller.Entrances[0].IsHighlighted);
        }
    }
}
