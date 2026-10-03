using System.Collections;
using System.Collections.Generic;
using FGJ.LiarDice;
using FGJ.LiarDice.Table;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FGJ.Tests.PlayMode.LiarDice
{
    public class DiceTableViewTests
    {
        private static readonly int[] DiceSequence = { 1, 4, 4, 2, 6, 3, 4, 5, 5, 1 };

        private readonly DieFaceLayout _faceLayout = new DieFaceLayout();
        private DiceTableView _table;

        [SetUp]
        public void SetUp()
        {
            _table = TestPrefabs.Instantiate<DiceTableView>(TestPrefabs.DiceTablePath);
            _table.SetAnimationDurations(0f, 0f);
            _table.SetYawRandom(new System.Random(11));
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_table.gameObject);
        }

        private static LiarDiceMatch StartMatch(int diceEach = 5)
        {
            var rolls = new List<int>();
            while (rolls.Count < diceEach * 2)
                rolls.AddRange(DiceSequence);
            var match = new LiarDiceMatch(new MatchSettings(diceEach, diceEach, 2, 2),
                new CyclingDiceRoller(rolls.GetRange(0, diceEach * 2).ToArray()));
            match.Start();
            return match;
        }

        private IEnumerator Play(IEnumerator routine)
        {
            var done = false;
            _table.StartCoroutine(Track(routine, () => done = true));
            for (var frame = 0; frame < 30 && !done; frame++)
                yield return null;
            Assert.IsTrue(done, "動畫逾時");
        }

        private static IEnumerator Track(IEnumerator routine, System.Action onDone)
        {
            yield return routine;
            onDone();
        }

        private void AssertDiceShow(IReadOnlyList<DieView> dice, IReadOnlyList<int> values)
        {
            Assert.AreEqual(values.Count, dice.Count);
            for (var i = 0; i < values.Count; i++)
            {
                Assert.AreEqual(values[i], dice[i].Value);
                Assert.AreEqual(values[i], _faceLayout.ValueFacingUp(dice[i].transform.localRotation), $"die {i}");
            }
        }

        [UnityTest]
        public IEnumerator PlayRoundStart_ShowsRolledFacesAndLiftsOnlyPlayerCup()
        {
            var match = StartMatch();

            yield return Play(_table.PlayRoundStart(match));

            AssertDiceShow(_table.PlayerDice, match.GetDice(Side.Player));
            AssertDiceShow(_table.MonsterDice, match.GetDice(Side.Monster));
            Assert.IsTrue(_table.PlayerCup.IsLifted);
            Assert.IsFalse(_table.MonsterCup.IsLifted);
        }

        [UnityTest]
        public IEnumerator PlayReveal_LiftsMonsterCupAndHighlightsMatchingDiceIncludingWild()
        {
            var match = StartMatch();
            yield return Play(_table.PlayRoundStart(match));
            match.PlaceBid(Side.Monster, new Bid(3, 4));
            match.Challenge(Side.Player);

            yield return Play(_table.PlayReveal(match));

            Assert.IsTrue(_table.MonsterCup.IsLifted);
            foreach (var die in _table.PlayerDice)
                Assert.AreEqual(die.Value == 4 || die.Value == 1, die.IsHighlighted, $"player die {die.Value}");
            foreach (var die in _table.MonsterDice)
                Assert.AreEqual(die.Value == 4 || die.Value == 1, die.IsHighlighted, $"monster die {die.Value}");
        }

        [UnityTest]
        public IEnumerator MoreDice_SpawnsExtraDieViews()
        {
            var match = StartMatch(8);

            yield return Play(_table.PlayRoundStart(match));

            Assert.AreEqual(8, _table.PlayerDice.Count);
            Assert.AreEqual(8, _table.MonsterDice.Count);
        }
    }
}
