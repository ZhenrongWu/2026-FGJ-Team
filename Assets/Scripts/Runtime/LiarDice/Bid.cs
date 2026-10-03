using System;

namespace FGJ.LiarDice
{
    public readonly struct Bid : IEquatable<Bid>
    {
        public readonly int Quantity;
        public readonly int Face;

        public Bid(int quantity, int face)
        {
            Quantity = quantity;
            Face = face;
        }

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
