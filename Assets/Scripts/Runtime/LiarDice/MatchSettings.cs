using System;

namespace FGJ.LiarDice
{
    public sealed class MatchSettings
    {
        public int PlayerDiceCount { get; }
        public int MonsterDiceCount { get; }
        public int PlayerOxygen { get; }
        public int PlayerMaxOxygen { get; }
        public int MonsterOxygen { get; }
        public Side FirstTurn { get; }
        public ILiarDiceRules Rules { get; }
        public int ItemCount { get; }

        public MatchSettings(int playerDiceCount, int monsterDiceCount, int playerOxygen, int monsterOxygen,
            Side firstTurn = Side.Monster, ILiarDiceRules rules = null, int itemCount = 0, int playerMaxOxygen = 0)
        {
            if (playerDiceCount < 1) throw new ArgumentOutOfRangeException(nameof(playerDiceCount));
            if (monsterDiceCount < 1) throw new ArgumentOutOfRangeException(nameof(monsterDiceCount));
            if (playerOxygen < 1) throw new ArgumentOutOfRangeException(nameof(playerOxygen));
            if (monsterOxygen < 1) throw new ArgumentOutOfRangeException(nameof(monsterOxygen));
            if (itemCount < 0) throw new ArgumentOutOfRangeException(nameof(itemCount));

            PlayerDiceCount = playerDiceCount;
            MonsterDiceCount = monsterDiceCount;
            PlayerOxygen = playerOxygen;
            PlayerMaxOxygen = Math.Max(playerOxygen, playerMaxOxygen);
            MonsterOxygen = monsterOxygen;
            FirstTurn = firstTurn;
            Rules = rules ?? new LiarDiceRules();
            ItemCount = itemCount;
        }
    }
}
