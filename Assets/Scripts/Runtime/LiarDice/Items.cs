using System;

namespace FGJ.LiarDice
{
    public enum ItemType
    {
        PeekLens,
        ExtraDie,
        Reroll,
        SealTape,
        StealOxygen
    }

    public enum ItemUseResult
    {
        Used,
        NotHeld,
        AlreadyActive
    }

    public readonly struct PeekResult : IEquatable<PeekResult>
    {
        public readonly int Face;
        public readonly int Count;

        public PeekResult(int face, int count)
        {
            Face = face;
            Count = count;
        }

        public bool Equals(PeekResult other) => Face == other.Face && Count == other.Count;
        public override bool Equals(object obj) => obj is PeekResult other && Equals(other);
        public override int GetHashCode() => Face * 31 + Count;
    }

    public interface IItemDealer
    {
        ItemType Draw();
    }

    public sealed class RandomItemDealer : IItemDealer
    {
        private readonly ItemType[] _pool = (ItemType[])Enum.GetValues(typeof(ItemType));
        private readonly Random _random;

        public RandomItemDealer() : this(new Random()) { }

        public RandomItemDealer(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public ItemType Draw() => _pool[_random.Next(_pool.Length)];
    }
}
