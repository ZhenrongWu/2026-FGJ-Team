using UnityEngine;

namespace FGJ.LiarDice
{
    [CreateAssetMenu(fileName = "Monster", menuName = "FGJ/Liar Dice/Monster Profile")]
    public sealed class MonsterProfile : ScriptableObject
    {
        [Header("顯示")]
        [SerializeField] private string displayName = "沼澤看守者";
        [Tooltip("留空時顯示佔位剪影")]
        [SerializeField] private Sprite sprite;
        [TextArea] [SerializeField] private string greeting = "「來吧，旅人。用你的氧氣，跟我賭一把。」";
        [SerializeField] private string challengeLine = "「你在吹牛。開！」";
        [Tooltip("{0} 會替換成喊數，例如「3 個 4 點」")]
        [SerializeField] private string bidLineFormat = "「{0}。」";

        [Header("AI 個性")]
        [Range(0f, 1f)] [SerializeField] private float challengeThreshold = 0.35f;
        [Range(0f, 1f)] [SerializeField] private float confidentBidThreshold = 0.5f;
        [Range(0f, 1f)] [SerializeField] private float bluffChance = 0.15f;
        [Min(1)] [SerializeField] private int maxRaiseStep = 2;

        public string DisplayName => displayName;
        public Sprite Sprite => sprite;
        public string Greeting => greeting;
        public string ChallengeLine => challengeLine;

        public string BidLine(Bid bid) => string.Format(bidLineFormat, bid);

        public MonsterAIProfile ToAIProfile()
        {
            return new MonsterAIProfile(challengeThreshold, confidentBidThreshold, bluffChance, maxRaiseStep);
        }

        public void SetSprite(Sprite monsterSprite)
        {
            sprite = monsterSprite;
        }

        public void Configure(string name, string greetingLine, string challenge, string bidFormat)
        {
            displayName = name;
            greeting = greetingLine;
            challengeLine = challenge;
            bidLineFormat = bidFormat;
        }
    }
}
