using System;

namespace FGJ.LiarDice
{
    /// <summary>一場對局的數值設定，由 <see cref="LiarDiceConfig"/> 產生。</summary>
    public sealed class MatchSettings
    {
        public int PlayerDiceCount { get; }
        public int MonsterDiceCount { get; }
        public int PlayerOxygen { get; }
        public int MonsterOxygen { get; }
        public Side FirstTurn { get; }

        public MatchSettings(int playerDiceCount, int monsterDiceCount, int playerOxygen, int monsterOxygen,
            Side firstTurn = Side.Monster)
        {
            if (playerDiceCount < 1) throw new ArgumentOutOfRangeException(nameof(playerDiceCount));
            if (monsterDiceCount < 1) throw new ArgumentOutOfRangeException(nameof(monsterDiceCount));
            if (playerOxygen < 1) throw new ArgumentOutOfRangeException(nameof(playerOxygen));
            if (monsterOxygen < 1) throw new ArgumentOutOfRangeException(nameof(monsterOxygen));

            PlayerDiceCount = playerDiceCount;
            MonsterDiceCount = monsterDiceCount;
            PlayerOxygen = playerOxygen;
            MonsterOxygen = monsterOxygen;
            FirstTurn = firstTurn;
        }
    }
}
