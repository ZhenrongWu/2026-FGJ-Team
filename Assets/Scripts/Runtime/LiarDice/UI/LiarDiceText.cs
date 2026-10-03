namespace FGJ.LiarDice.UI
{
    public static class LiarDiceText
    {
        public const string MonsterName = "沼澤看守者";
        public const string MonsterGreeting = "「來吧，旅人。用你的氧氣，跟我賭一把。」";
        public const string MonsterChallengeLine = "「你在吹牛。開！」";
        public const string NotPlayerTurn = "還沒輪到你。";
        public const string NothingToChallenge = "目前沒有可以質疑的喊數。";
        public const string NextRoundLabel = "下一局";
        public const string RematchLabel = "再挑戰一次";

        public static string SideName(Side side) => side == Side.Player ? "你" : "怪物";

        public static string CurrentBid(Bid? bid, Side? bidder)
        {
            if (!bid.HasValue || !bidder.HasValue)
                return "尚未喊數";
            return $"{SideName(bidder.Value)}喊：{bid.Value}";
        }

        public static string WildStatus(bool wildActive) => wildActive ? "1 點萬用：有效" : "1 點萬用：已取消";

        public static string Oxygen(string owner, int current, int max)
        {
            var remaining = current < 0 ? 0 : current;
            return $"{owner}氧氣 {new string('●', remaining)}{new string('○', max - remaining)}";
        }

        public static string TurnStatus(LiarDiceMatch match)
        {
            switch (match.Phase)
            {
                case MatchPhase.Bidding:
                    if (match.CurrentTurn == Side.Monster)
                        return "怪物思考中……";
                    return match.CanRaise ? "輪到你：相信並加注，或是質疑" : "已經喊到最大，只能質疑";
                case MatchPhase.RoundOver:
                    return "開盅！";
                case MatchPhase.MatchOver:
                    return "勝負已分";
                default:
                    return string.Empty;
            }
        }

        public static string MonsterBidLine(Bid bid) => $"「{bid}。」";

        public static string RoundResult(RoundResult result, bool wildActive)
        {
            var wildNote = wildActive && result.Bid.Face != LiarDiceRules.WildFace ? "（含萬用 1 點）" : string.Empty;
            var verdict = result.Loser == Side.Player ? "你輸了，扣一格氧氣。" : "怪物輸了，扣一格氧氣。";
            return $"{SideName(result.Challenger)}質疑「{result.Bid}」\n全場符合 {result.ActualCount} 顆{wildNote}\n{verdict}";
        }

        public static string MatchOver(Side winner)
        {
            return winner == Side.Player ? "怪物的氧氣耗盡，你贏了！" : "你的氧氣耗盡了……";
        }
    }
}
