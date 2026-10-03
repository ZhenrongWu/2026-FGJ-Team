using UnityEngine;

namespace FGJ.LiarDice
{
    [CreateAssetMenu(fileName = "LiarDiceConfig", menuName = "FGJ/Liar Dice Config")]
    public sealed class LiarDiceConfig : ScriptableObject
    {
        [Header("骰子")]
        [Min(1)] [SerializeField] private int playerDiceCount = 5;
        [Min(1)] [SerializeField] private int monsterDiceCount = 5;

        [Header("氧氣")]
        [Min(1)] [SerializeField] private int playerOxygen = 2;
        [Min(1)] [SerializeField] private int monsterOxygen = 2;

        [Header("規則")]
        [SerializeField] private bool onesAreWild = true;

        [Header("回合")]
        [SerializeField] private Side firstTurn = Side.Monster;

        public int PlayerDiceCount => playerDiceCount;
        public int MonsterDiceCount => monsterDiceCount;
        public int PlayerOxygen => playerOxygen;
        public int MonsterOxygen => monsterOxygen;
        public Side FirstTurn => firstTurn;

        public bool OnesAreWild => onesAreWild;

        public ILiarDiceRules ToRules() => new LiarDiceRules(onesAreWild);

        public MatchSettings ToMatchSettings()
        {
            return new MatchSettings(playerDiceCount, monsterDiceCount, playerOxygen, monsterOxygen, firstTurn,
                ToRules());
        }
    }
}
