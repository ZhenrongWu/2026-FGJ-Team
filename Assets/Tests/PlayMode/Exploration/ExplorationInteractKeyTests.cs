using System.Collections;
using NUnit.Framework;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
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

        private IEnumerator Tap(KeyControl key)
        {
            Press(key);
            yield return null;
            Release(key);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PressingUpArrow_EntersBuilding()
        {
            yield return StandAtBuilding();

            yield return Tap(_keyboard.upArrowKey);

            Assert.IsTrue(_rig.Controller.IsEnteringRoom);
        }

        [UnityTest]
        public IEnumerator PressingW_EntersBuilding()
        {
            yield return StandAtBuilding();

            yield return Tap(_keyboard.wKey);

            Assert.IsTrue(_rig.Controller.IsEnteringRoom);
        }

        [UnityTest]
        public IEnumerator PressingE_DoesNotEnterBuilding()
        {
            yield return StandAtBuilding();

            yield return Tap(_keyboard.eKey);

            Assert.IsFalse(_rig.Controller.IsEnteringRoom);
            Assert.IsEmpty(_rig.Router.LoadedScenes);
        }
    }
}
