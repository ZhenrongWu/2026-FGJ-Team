using FGJ.LiarDice;

namespace FGJ.Tests.PlayMode.LiarDice
{
    internal sealed class CyclingDiceRoller : IDiceRoller
    {
        private readonly int[] _values;
        private int _index;

        public CyclingDiceRoller(int[] values)
        {
            _values = values;
        }

        public int Roll()
        {
            var value = _values[_index];
            _index = (_index + 1) % _values.Length;
            return value;
        }
    }
}
