using System.Collections;
using NUnit.Framework;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace FGJ.Tests.PlayMode.Exploration
{
    public class ExplorationInteractKeyTests : InputTestFixture
    {
        private ExplorationTestRig _rig;
        private Keyboard _keyboard;

        public override void Setup()
        {
            base.Setup();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _rig = new ExplorationTestRig();
        }

        public override void TearDown()
        {
            _rig.Dispose();
            base.TearDown();
        }

        private IEnumerator StandAtBuilding()
        {
            _rig.Build(ExplorationTestRig.BuildingX);
            yield return null;
            yield return null;
            Assert.AreEqual(ExplorationTestRig.EnterPrompt, _rig.Hud.PromptMessage);
        }

        [UnityTest]
        public IEnumerator PressingE_EntersBuilding()
        {
            yield return StandAtBuilding();

            Press(_keyboard.eKey);
            yield return null;
            Release(_keyboard.eKey);
            yield return null;

            Assert.IsTrue(_rig.Controller.IsEnteringRoom);
        }

        [UnityTest]
        public IEnumerator PressingUpArrowOrW_DoesNotEnterBuilding()
        {
            yield return StandAtBuilding();

            Press(_keyboard.upArrowKey);
            yield return null;
            Release(_keyboard.upArrowKey);
            Press(_keyboard.wKey);
            yield return null;
            Release(_keyboard.wKey);
            yield return null;

            Assert.IsFalse(_rig.Controller.IsEnteringRoom);
            Assert.IsEmpty(_rig.Router.LoadedScenes);
        }
    }
}
