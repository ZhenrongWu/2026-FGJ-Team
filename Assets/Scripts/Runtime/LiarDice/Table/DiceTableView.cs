using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FGJ.LiarDice.Table
{
    public sealed class DiceTableView : MonoBehaviour
    {
        [SerializeField] private DiceCupView playerCup;
        [SerializeField] private DiceCupView monsterCup;
        [SerializeField] private DieView diePrefab;
        [Min(0f)] [SerializeField] private float shakeDuration = 0.7f;
        [Min(0f)] [SerializeField] private float liftDuration = 0.45f;

        private readonly List<DieView> _playerDice = new List<DieView>();
        private readonly List<DieView> _monsterDice = new List<DieView>();
        private readonly DieFaceLayout _faceLayout = new DieFaceLayout();
        private readonly TrayLayout _trayLayout = new TrayLayout();
        private System.Random _yawRandom = new System.Random();

        public IReadOnlyList<DieView> PlayerDice => _playerDice;
        public IReadOnlyList<DieView> MonsterDice => _monsterDice;
        public DiceCupView PlayerCup => playerCup;
        public DiceCupView MonsterCup => monsterCup;

        public void Configure(DiceCupView player, DiceCupView monster, DieView dieTemplate)
        {
            playerCup = player;
            monsterCup = monster;
            diePrefab = dieTemplate;
        }

        public void SetAnimationDurations(float shake, float lift)
        {
            shakeDuration = Mathf.Max(0f, shake);
            liftDuration = Mathf.Max(0f, lift);
        }

        public void SetYawRandom(System.Random random)
        {
            _yawRandom = random ?? new System.Random();
        }

        public IEnumerator PlayRoundStart(LiarDiceMatch match)
        {
            playerCup.SetLiftedImmediate(false);
            monsterCup.SetLiftedImmediate(false);
            ClearHighlights();

            if (shakeDuration > 0f)
            {
                var monsterShake = StartCoroutine(monsterCup.Shake(shakeDuration));
                yield return playerCup.Shake(shakeDuration);
                yield return monsterShake;
            }

            ShowDice(match);
            yield return playerCup.AnimateLift(true, liftDuration);
        }

        public IEnumerator PlayReveal(LiarDiceMatch match)
        {
            playerCup.SetLiftedImmediate(true);
            yield return monsterCup.AnimateLift(true, liftDuration);
            HighlightMatches(match);
        }

        public void ShowDice(LiarDiceMatch match)
        {
            Arrange(_playerDice, playerCup, match.GetDice(Side.Player));
            Arrange(_monsterDice, monsterCup, match.GetDice(Side.Monster));
        }

        private void Arrange(List<DieView> dice, DiceCupView cup, IReadOnlyList<int> values)
        {
            while (dice.Count < values.Count)
                dice.Add(Instantiate(diePrefab, cup.DiceRoot));

            var positions = _trayLayout.Positions(values.Count, cup.TrayRadius);
            for (var i = 0; i < dice.Count; i++)
            {
                var visible = i < values.Count;
                dice[i].gameObject.SetActive(visible);
                if (!visible)
                    continue;

                dice[i].transform.localPosition = positions[i];
                dice[i].ShowValue(values[i], (float)_yawRandom.NextDouble() * 360f, _faceLayout);
                dice[i].SetHighlighted(false);
            }
        }

        private void HighlightMatches(LiarDiceMatch match)
        {
            if (!match.CurrentBid.HasValue)
                return;
            var face = match.CurrentBid.Value.Face;
            Highlight(_playerDice, match, face);
            Highlight(_monsterDice, match, face);
        }

        private void Highlight(List<DieView> dice, LiarDiceMatch match, int face)
        {
            foreach (var die in dice)
            {
                if (!die.gameObject.activeSelf)
                    continue;
                die.SetHighlighted(match.Rules.CountMatching(new[] { die.Value }, face, match.WildActive) > 0);
            }
        }

        private void ClearHighlights()
        {
            foreach (var die in _playerDice)
                die.SetHighlighted(false);
            foreach (var die in _monsterDice)
                die.SetHighlighted(false);
        }
    }
}
