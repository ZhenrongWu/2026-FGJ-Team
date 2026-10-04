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
        [Tooltip("玩家氧氣上限，也是第一次進入時的氧氣")]
        [Min(1)] [SerializeField] private int playerOxygen = 5;
        [Min(1)] [SerializeField] private int monsterOxygen = 2;
        [Tooltip("獲勝後回復的氧氣，不會超過上限")]
        [Min(0)] [SerializeField] private int winOxygenReward = 2;

        [Header("道具")]
        [Tooltip("開局時每人抽到的道具數量，可重複")]
        [Min(0)] [SerializeField] private int itemCount;

        [Header("規則")]
        [SerializeField] private bool onesAreWild = true;

        [Header("回合")]
        [SerializeField] private Side firstTurn = Side.Monster;

        [Header("音樂")]
        [SerializeField] private AudioClip music;

        [Header("流程")]
        [Tooltip("勾選後，獲勝會進入結局而不是繼續探索")]
        [SerializeField] private bool endsGame;

        public int PlayerDiceCount => playerDiceCount;
        public int MonsterDiceCount => monsterDiceCount;
        public int PlayerOxygen => playerOxygen;
        public int MonsterOxygen => monsterOxygen;
        public int WinOxygenReward => winOxygenReward;
        public int ItemCount => itemCount;
        public Side FirstTurn => firstTurn;
        public bool EndsGame => endsGame;
        public AudioClip Music => music;

        public bool OnesAreWild => onesAreWild;

        public ILiarDiceRules ToRules() => new LiarDiceRules(onesAreWild);

        public MatchSettings ToMatchSettings() => ToMatchSettings(playerOxygen);

        public MatchSettings ToMatchSettings(int playerStartOxygen)
        {
            return new MatchSettings(playerDiceCount, monsterDiceCount, Mathf.Clamp(playerStartOxygen, 1, playerOxygen),
                monsterOxygen, firstTurn, ToRules(), itemCount, playerOxygen);
        }

        public int OxygenAfterVictory(int remainingOxygen)
        {
            return Mathf.Min(playerOxygen, remainingOxygen + winOxygenReward);
        }

        public void SetMusic(AudioClip clip)
        {
            music = clip;
        }

        public void Configure(int diceCount, int monsterStartOxygen, int itemsPerSide, bool finalLevel)
        {
            playerDiceCount = diceCount;
            monsterDiceCount = diceCount;
            monsterOxygen = monsterStartOxygen;
            itemCount = itemsPerSide;
            endsGame = finalLevel;
        }
    }
}
