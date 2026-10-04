namespace FGJ.LiarDice.UI
{
    public sealed class LiarDiceText
    {
        public const string NotPlayerTurn = "還沒輪到你。";
        public const string NothingToChallenge = "目前沒有可以質疑的喊數。";
        public const string BelieveFirst = "請先按「相信」再喊數。";
        public const string ItemAlreadyActive = "這個道具已經啟動了。";
        public const string NextRoundLabel = "下一局";
        public const string RematchLabel = "再挑戰一次";
        public const string ContinueExploringLabel = "繼續探索";
        public const string RetryLabel = "重新挑戰";
        public const string EndingLabel = "觀看結局";
        public const string HiddenItemLabel = "？";

        public string MatchOverLabel(Side winner, bool leavesRoom, bool endsGame = false)
        {
            if (!leavesRoom)
                return RematchLabel;
            if (winner != Side.Player)
                return RetryLabel;
            return endsGame ? EndingLabel : ContinueExploringLabel;
        }

        public string SideName(Side side) => side == Side.Player ? "你" : "怪物";

        public string ItemName(ItemType item)
        {
            switch (item)
            {
                case ItemType.PeekLens:
                    return "偷窺鏡片";
                case ItemType.ExtraDie:
                    return "偷加骰子";
                case ItemType.Reroll:
                    return "重搖";
                case ItemType.SealTape:
                    return "封口膠帶";
                default:
                    return "偷走氧氣";
            }
        }

        public string ItemDescription(ItemType item)
        {
            switch (item)
            {
                case ItemType.PeekLens:
                    return "下一局開始時，隨機得知對方某個點數有幾顆。";
                case ItemType.ExtraDie:
                    return "在對方不知情下，自己的骰盅多一顆骰子。";
                case ItemType.Reroll:
                    return "雙方立刻重新搖骰，喊數保留。";
                case ItemType.SealTape:
                    return "對方下一個回合被跳過，你要再喊一次更大的數。";
                default:
                    return "這次開盅若你獲勝，對方多扣一格氧氣，你回復一格。";
            }
        }

        public string ItemTooltip(ItemType item) => $"{ItemName(item)}：{ItemDescription(item)}";

        public string ItemUsed(Side side, ItemType item)
        {
            switch (item)
            {
                case ItemType.PeekLens:
                    return side == Side.Player ? "你戴上偷窺鏡片，下一局開始時生效。" : "怪物戴上了偷窺鏡片……";
                case ItemType.ExtraDie:
                    return "你趁對方不注意，偷偷在骰盅裡多加了一顆骰子。";
                case ItemType.Reroll:
                    return $"{SideName(side)}使用重搖，雙方重新搖骰！";
                case ItemType.SealTape:
                    return side == Side.Player ? "你用封口膠帶封住了怪物的嘴。" : "怪物拿出封口膠帶封住了你的嘴！";
                default:
                    return $"{SideName(side)}啟動了偷走氧氣，這次開盅的贏家會多拿一格。";
            }
        }

        public bool AnnouncesUse(Side side, ItemType item) => side == Side.Player || item != ItemType.ExtraDie;

        public string TurnSkipped(Side side)
        {
            return side == Side.Player ? "你被封口膠帶封住，跳過這一回合。" : "怪物被封口膠帶封住，跳過這一回合。";
        }

        public string PeekReveal(PeekResult peek) => $"偷窺鏡片：怪物有 {peek.Count} 顆 {peek.Face} 點。";

        public string CurrentBid(Bid? bid, Side? bidder)
        {
            if (!bid.HasValue || !bidder.HasValue)
                return "尚未喊數";
            return $"{SideName(bidder.Value)}喊：{bid.Value}";
        }

        public string WildStatus(bool wildActive) => wildActive ? "1 點萬用：有效" : "1 點萬用：已取消";

        public string Status(LiarDiceMatch match)
        {
            var peek = match.GetPeek(Side.Player);
            var peekNote = peek.HasValue ? $"｜偷窺：{peek.Value.Count} 顆 {peek.Value.Face} 點" : string.Empty;
            return $"{CurrentBid(match.CurrentBid, match.CurrentBidder)}｜{WildStatus(match.WildActive)}{peekNote}\n{TurnStatus(match)}";
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
                    if (match.MustRaise)
                        return "怪物被封口了：再喊一次更大的數";
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
            var stealNote = result.OxygenStolen
                ? $"{SideName(result.Winner)}偷走一格氧氣，{SideName(result.Loser)}再扣一格！"
                : string.Empty;
            return $"{SideName(result.Challenger)}喊吹牛，開盅「{result.Bid}」：全場 {result.ActualCount} 顆{wildNote}。{verdict}{stealNote}";
        }

        public string MatchOver(Side winner)
        {
            return winner == Side.Player ? "怪物的氧氣耗盡，你贏了！" : "你的氧氣耗盡了……";
        }
    }
}
