using System.Collections.Generic;
using FGJ.LiarDice;

namespace FGJ.Tests.EditMode.LiarDice
{
    internal sealed class FixedDiceRoller : IDiceRoller
    {
        private readonly Queue<int> _values;

        public FixedDiceRoller(params int[] values)
        {
            _values = new Queue<int>(values);
        }

        public int Roll() => _values.Dequeue();
    }
}
