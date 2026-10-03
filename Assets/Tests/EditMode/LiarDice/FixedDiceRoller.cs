using System.Collections.Generic;
using FGJ.LiarDice;

namespace FGJ.Tests.EditMode.LiarDice
{
    /// <summary>依序回傳指定點數的骰子，讓測試結果可預期。</summary>
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
