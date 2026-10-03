using System;

namespace FGJ.LiarDice
{
    /// <summary>喊數：「Quantity 個 Face 點」。</summary>
    public readonly struct Bid : IEquatable<Bid>
    {
        public readonly int Quantity;
        public readonly int Face;

        public Bid(int quantity, int face)
        {
            Quantity = quantity;
            Face = face;
        }

        /// <summary>數量更多，或數量相同但點數更大（1 &lt; 2 &lt; … &lt; 6）。</summary>
        public bool IsHigherThan(Bid other)
        {
            return Quantity > other.Quantity || (Quantity == other.Quantity && Face > other.Face);
        }

        public bool Equals(Bid other) => Quantity == other.Quantity && Face == other.Face;
        public override bool Equals(object obj) => obj is Bid other && Equals(other);
        public override int GetHashCode() => Quantity * 31 + Face;
        public static bool operator ==(Bid a, Bid b) => a.Equals(b);
        public static bool operator !=(Bid a, Bid b) => !a.Equals(b);

        public override string ToString() => $"{Quantity} 個 {Face} 點";
    }
}
