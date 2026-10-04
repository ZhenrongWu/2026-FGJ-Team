using System;
using System.Collections;
using System.Linq;
using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace FGJ.Tests.PlayMode.LiarDice
{
    public class LiarDiceRoomItemTests
    {
        private const int FrameLimit = 60;
        private static readonly int[] DiceSequence = { 1, 4, 4, 2, 6, 3, 4, 5, 5, 1 };

        private readonly LiarDiceText _text = new LiarDiceText();
        private LiarDiceRoomController _controller;

        private LiarDiceHud Hud => _controller.Hud;
        private LiarDiceMatch Match => _controller.Match;

        [SetUp]
        public void SetUp()
        {
            _controller = TestPrefabs.Instantiate<LiarDiceRoomController>(TestPrefabs.LiarDiceRoomPath);
            _controller.SetMonsterThinkSeconds(0f);
            _controller.Table.SetAnimationDurations(0f, 0f);
            _controller.Table.SetYawRandom(new System.Random(3));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_controller.gameObject);
        }

        private void Begin(ItemType[] playerItems, ItemType[] monsterItems)
        {
            var profile = new MonsterAIProfile(0.35f, 0.5f, 0f, 2);
            var dealt = playerItems.Concat(monsterItems).ToArray();
            _controller.Begin(new MatchSettings(5, 5, 2, 2, Side.Monster, null, playerItems.Length),
                () => new CyclingDiceRoller(DiceSequence), new MonsterAI(profile, new System.Random(7)),
                () => new QueuedItemDealer(dealt));
        }

        private static ItemType[] Items(params ItemType[] items) => items;

        private static IEnumerator WaitUntil(Func<bool> condition)
        {
            for (var frame = 0; frame < FrameLimit && !condition(); frame++)
                yield return null;
            Assert.IsTrue(condition(), "等待逾時");
        }

        private IEnumerator WaitForPlayerTurn() => WaitUntil(() => _controller.IsPlayerTurn);

        private bool LogContains(string text) => _controller.Log.Entries.Any(entry => entry.Text.Contains(text));

        [UnityTest]
        public IEnumerator PlayerTurn_ItemBarShowsOwnItemsAndHidesMonsters()
        {
            Begin(Items(ItemType.SealTape, ItemType.Reroll), Items(ItemType.Reroll, ItemType.Reroll));
            yield return WaitForPlayerTurn();

            CollectionAssert.AreEqual(new[] { "封口膠帶", "重搖" }, Hud.PlayerItems.Labels);
            Assert.IsTrue(Hud.PlayerItems.IsInteractable);
            CollectionAssert.AreEqual(new[] { LiarDiceText.HiddenItemLabel, LiarDiceText.HiddenItemLabel },
                Hud.MonsterItems.Labels);
            Assert.IsFalse(Hud.MonsterItems.IsInteractable);
            Assert.IsTrue(Hud.MonsterItems.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator ClickingItemSlot_UsesItemAndLogsIt()
        {
            Begin(Items(ItemType.SealTape, ItemType.Reroll), Items(ItemType.Reroll, ItemType.Reroll));
            yield return WaitForPlayerTurn();

            Hud.PlayerItems.Click(0);

            Assert.IsTrue(Match.IsSealed(Side.Monster));
            Assert.AreEqual(_text.ItemUsed(Side.Player, ItemType.SealTape), Hud.LogView.LatestText);
            CollectionAssert.AreEqual(new[] { "重搖" }, Hud.PlayerItems.Labels);
        }

        [Test]
        public void UsePlayerItem_BeforePlayerTurn_IsRejected()
        {
            Begin(Items(ItemType.SealTape), Items(ItemType.Reroll));

            var result = _controller.UsePlayerItem(ItemType.SealTape);

            Assert.AreEqual(ItemUseResult.NotHeld, result);
            Assert.AreEqual(LiarDiceText.NotPlayerTurn, Hud.ErrorMessage);
            Assert.AreEqual(1, Match.GetItems(Side.Player).Count);
        }

        [UnityTest]
        public IEnumerator SealTapeThenBid_SkipsMonsterAndPlayerMustRaise()
        {
            Begin(Items(ItemType.SealTape), Items(ItemType.Reroll));
            yield return WaitForPlayerTurn();
            _controller.UsePlayerItem(ItemType.SealTape);
            var raise = new Bid(Match.CurrentBid.Value.Quantity + 1, Match.CurrentBid.Value.Face);
            _controller.Believe();

            _controller.SubmitPlayerBid(raise.Quantity.ToString(), raise.Face.ToString());

            Assert.AreEqual(_text.TurnSkipped(Side.Monster), Hud.LogView.LatestText);
            Assert.IsTrue(_controller.IsPlayerTurn);
            Assert.IsTrue(Hud.IsBidInputEnabled);
            Assert.IsFalse(Hud.IsBluffEnabled);
            Assert.IsFalse(Hud.IsBelieveEnabled);
        }

        [UnityTest]
        public IEnumerator SealTape_UsedTwice_ShowsAlreadyActive()
        {
            Begin(Items(ItemType.SealTape, ItemType.SealTape), Items(ItemType.Reroll, ItemType.Reroll));
            yield return WaitForPlayerTurn();
            _controller.UsePlayerItem(ItemType.SealTape);

            Assert.AreEqual(ItemUseResult.AlreadyActive, _controller.UsePlayerItem(ItemType.SealTape));
            Assert.AreEqual(LiarDiceText.ItemAlreadyActive, Hud.ErrorMessage);
        }

        [UnityTest]
        public IEnumerator Reroll_ByPlayer_ShakesCupsKeepsBidAndReturnsControl()
        {
            Begin(Items(ItemType.Reroll), Items(ItemType.Reroll));
            yield return WaitForPlayerTurn();
            var bid = Match.CurrentBid;

            _controller.UsePlayerItem(ItemType.Reroll);
            yield return WaitForPlayerTurn();

            Assert.AreEqual(bid, Match.CurrentBid);
            Assert.AreEqual(_text.ItemUsed(Side.Player, ItemType.Reroll), Hud.LogView.LatestText);
            CollectionAssert.AreEqual(Match.GetDice(Side.Player),
                _controller.Table.PlayerDice.Where(die => die.gameObject.activeSelf).Select(die => die.Value));
            Assert.IsTrue(_controller.Table.PlayerCup.IsLifted);
        }

        [UnityTest]
        public IEnumerator ExtraDie_ByPlayer_ShowsSixDiceInPlayerCup()
        {
            Begin(Items(ItemType.ExtraDie), Items(ItemType.Reroll));
            yield return WaitForPlayerTurn();

            _controller.UsePlayerItem(ItemType.ExtraDie);

            Assert.AreEqual(6, Match.GetDice(Side.Player).Count);
            Assert.AreEqual(6, _controller.Table.PlayerDice.Count(die => die.gameObject.activeSelf));
        }

        [UnityTest]
        public IEnumerator ExtraDie_ByMonster_IsUsedSecretly()
        {
            Begin(Items(ItemType.Reroll), Items(ItemType.ExtraDie));
            yield return WaitForPlayerTurn();

            Assert.AreEqual(6, Match.GetDice(Side.Monster).Count);
            Assert.AreEqual(5, Match.VisibleDiceCount(Side.Player, Side.Monster));
            Assert.IsFalse(LogContains(_text.ItemUsed(Side.Monster, ItemType.ExtraDie)));
            Assert.AreEqual(0, Hud.MonsterItems.VisibleCount);
        }

        [UnityTest]
        public IEnumerator PeekLens_RevealsMonsterFaceCountNextRound()
        {
            Begin(Items(ItemType.PeekLens), Items(ItemType.Reroll));
            yield return WaitForPlayerTurn();
            _controller.UsePlayerItem(ItemType.PeekLens);
            _controller.ChallengeAsPlayer();
            yield return WaitUntil(() => !_controller.IsBusy);

            _controller.Continue();

            var peek = Match.GetPeek(Side.Player);
            Assert.IsTrue(peek.HasValue);
            Assert.IsTrue(LogContains(_text.PeekReveal(peek.Value)));
            StringAssert.Contains("偷窺：", Hud.StatusMessage);
        }
    }
}
