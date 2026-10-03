namespace FGJ.LiarDice.UI
{
    public sealed class LiarDiceText
    {
        public const string NotPlayerTurn = "還沒輪到你。";
        public const string NothingToChallenge = "目前沒有可以質疑的喊數。";
        public const string BelieveFirst = "請先按「相信」再喊數。";
        public const string NextRoundLabel = "下一局";
        public const string RematchLabel = "再挑戰一次";
        public const string ReturnToCaveLabel = "返回洞穴";
        public const string RestartLabel = "重新開始";

        public string MatchOverLabel(Side winner, bool leavesRoom)
        {
            if (!leavesRoom)
                return RematchLabel;
            return winner == Side.Player ? ReturnToCaveLabel : RestartLabel;
        }

        public string SideName(Side side) => side == Side.Player ? "你" : "怪物";

        public string CurrentBid(Bid? bid, Side? bidder)
        {
            if (!bid.HasValue || !bidder.HasValue)
                return "尚未喊數";
            return $"{SideName(bidder.Value)}喊：{bid.Value}";
        }

        public string WildStatus(bool wildActive) => wildActive ? "1 點萬用：有效" : "1 點萬用：已取消";

        public string Status(LiarDiceMatch match)
        {
            return $"{CurrentBid(match.CurrentBid, match.CurrentBidder)}｜{WildStatus(match.WildActive)}\n{TurnStatus(match)}";
        }

        public string TurnStatus(LiarDiceMatch match)
        {
            switch (match.Phase)
            {
                case MatchPhase.Bidding:
                    if (match.CurrentTurn == Side.Monster)
                        return "怪物思考中……";
                    if (!match.CurrentBid.HasValue)
                        return "輪到你先喊數";
                    return match.CanRaise ? "輪到你：相信後加注，或喊吹牛開盅" : "已經喊到最大，只能喊吹牛開盅";
                case MatchPhase.RoundOver:
                    return "開盅！";
                case MatchPhase.MatchOver:
                    return "勝負已分";
                default:
                    return string.Empty;
            }
        }

        public string Speech(string speaker, string line) => $"{speaker}：{line}";

        public string PlayerBid(Bid bid, bool believedPrevious)
        {
            return believedPrevious ? $"你相信，喊：{bid}" : $"你喊：{bid}";
        }

        public string RoundStart(int roundNumber, Side starter) => $"— 第 {roundNumber} 局，{SideName(starter)}先喊 —";

        public string RoundResult(RoundResult result, bool wildCounted)
        {
            var wildNote = wildCounted ? "（含萬用 1 點）" : string.Empty;
            var verdict = result.Loser == Side.Player ? "你輸了，扣一格氧氣。" : "怪物輸了，扣一格氧氣。";
            return $"{SideName(result.Challenger)}喊吹牛，開盅「{result.Bid}」：全場 {result.ActualCount} 顆{wildNote}。{verdict}";
        }

        public string MatchOver(Side winner)
        {
            return winner == Side.Player ? "怪物的氧氣耗盡，你贏了！" : "你的氧氣耗盡了……";
        }
    }
}
