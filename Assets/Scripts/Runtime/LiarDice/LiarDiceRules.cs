using System.Collections.Generic;

namespace FGJ.LiarDice
{
    public enum Side
    {
        Player,
        Monster
    }

    public enum BidValidation
    {
        Valid,
        FaceOutOfRange,
        QuantityTooLow,
        QuantityTooHigh,
        NotHigher
    }

    public static class LiarDiceRules
    {
        public const int MinFace = 1;
        public const int MaxFace = 6;
        public const int WildFace = 1;

        public static Side Opponent(Side side) => side == Side.Player ? Side.Monster : Side.Player;

        public static bool CancelsWild(Bid bid) => bid.Face == WildFace;

        public static int CountMatching(IEnumerable<int> dice, int face, bool wildActive)
        {
            var count = 0;
            foreach (var die in dice)
            {
                if (die == face || (wildActive && face != WildFace && die == WildFace))
                    count++;
            }
            return count;
        }

        public static BidValidation Validate(Bid? currentBid, Bid next, int totalDice)
        {
            if (next.Face < MinFace || next.Face > MaxFace)
                return BidValidation.FaceOutOfRange;
            if (next.Quantity < 1)
                return BidValidation.QuantityTooLow;
            if (next.Quantity > totalDice)
                return BidValidation.QuantityTooHigh;
            if (currentBid.HasValue && !next.IsHigherThan(currentBid.Value))
                return BidValidation.NotHigher;
            return BidValidation.Valid;
        }

        public static bool CanRaise(Bid? currentBid, int totalDice)
        {
            if (!currentBid.HasValue)
                return totalDice > 0;
            return currentBid.Value.Quantity < totalDice || currentBid.Value.Face < MaxFace;
        }
    }
}
