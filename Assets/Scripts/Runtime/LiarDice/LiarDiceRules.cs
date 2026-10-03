using System.Collections.Generic;

namespace FGJ.LiarDice
{
    public enum Side
    {
        Player,
        Monster
    }

    public static class SideExtensions
    {
        public static Side Opponent(this Side side) => side == Side.Player ? Side.Monster : Side.Player;
    }

    public enum BidValidation
    {
        Valid,
        FaceOutOfRange,
        QuantityTooLow,
        QuantityTooHigh,
        NotHigher
    }

    public interface ILiarDiceRules
    {
        bool OnesAreWild { get; }
        bool CancelsWild(Bid bid);
        bool IsWildFor(int face, bool wildActive);
        int CountMatching(IEnumerable<int> dice, int face, bool wildActive);
        BidValidation Validate(Bid? currentBid, Bid next, int totalDice);
        bool CanRaise(Bid? currentBid, int totalDice);
    }

    public sealed class LiarDiceRules : ILiarDiceRules
    {
        public const int MinFace = 1;
        public const int MaxFace = 6;
        public const int WildFace = 1;

        public bool OnesAreWild { get; }

        public LiarDiceRules(bool onesAreWild = true)
        {
            OnesAreWild = onesAreWild;
        }

        public bool CancelsWild(Bid bid) => OnesAreWild && bid.Face == WildFace;

        public bool IsWildFor(int face, bool wildActive) => OnesAreWild && wildActive && face != WildFace;

        public int CountMatching(IEnumerable<int> dice, int face, bool wildActive)
        {
            var wildCounts = IsWildFor(face, wildActive);
            var count = 0;
            foreach (var die in dice)
            {
                if (die == face || (wildCounts && die == WildFace))
                    count++;
            }
            return count;
        }

        public BidValidation Validate(Bid? currentBid, Bid next, int totalDice)
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

        public bool CanRaise(Bid? currentBid, int totalDice)
        {
            if (!currentBid.HasValue)
                return totalDice > 0;
            return currentBid.Value.Quantity < totalDice || currentBid.Value.Face < MaxFace;
        }
    }
}
