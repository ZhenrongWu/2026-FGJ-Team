namespace FGJ.LiarDice.UI
{
    public readonly struct BidPanelState
    {
        public readonly bool BelieveEnabled;
        public readonly bool BluffEnabled;
        public readonly bool BidInputEnabled;
        public readonly bool ContinueVisible;
        public readonly bool ItemsEnabled;

        public BidPanelState(bool believeEnabled, bool bluffEnabled, bool bidInputEnabled, bool continueVisible,
            bool itemsEnabled = false)
        {
            BelieveEnabled = believeEnabled;
            BluffEnabled = bluffEnabled;
            BidInputEnabled = bidInputEnabled;
            ContinueVisible = continueVisible;
            ItemsEnabled = itemsEnabled;
        }
    }

    public sealed class BidPanelRules
    {
        public BidPanelState Evaluate(LiarDiceMatch match, bool believed, bool busy)
        {
            if (match == null)
                return default;

            if (match.Phase == MatchPhase.RoundOver || match.Phase == MatchPhase.MatchOver)
                return new BidPanelState(false, false, false, !busy);

            var playerCanAct = !busy && match.Phase == MatchPhase.Bidding && match.CurrentTurn == Side.Player;
            if (!playerCanAct)
                return default;

            var hasItems = match.GetItems(Side.Player).Count > 0;
            if (!match.CurrentBid.HasValue)
                return new BidPanelState(false, false, true, false, hasItems);
            if (match.MustRaise)
                return new BidPanelState(false, false, match.CanRaise, false, hasItems);

            var canRaise = match.CanRaise;
            return new BidPanelState(canRaise && !believed, match.CanChallenge, canRaise && believed, false, hasItems);
        }
    }
}
