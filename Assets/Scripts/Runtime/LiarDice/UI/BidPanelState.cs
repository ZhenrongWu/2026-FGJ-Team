namespace FGJ.LiarDice.UI
{
    public readonly struct BidPanelState
    {
        public readonly bool BelieveEnabled;
        public readonly bool BluffEnabled;
        public readonly bool BidInputEnabled;
        public readonly bool ContinueVisible;

        public BidPanelState(bool believeEnabled, bool bluffEnabled, bool bidInputEnabled, bool continueVisible)
        {
            BelieveEnabled = believeEnabled;
            BluffEnabled = bluffEnabled;
            BidInputEnabled = bidInputEnabled;
            ContinueVisible = continueVisible;
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

            if (!match.CurrentBid.HasValue)
                return new BidPanelState(false, false, true, false);

            var canRaise = match.CanRaise;
            return new BidPanelState(canRaise && !believed, match.CanChallenge, canRaise && believed, false);
        }
    }
}
