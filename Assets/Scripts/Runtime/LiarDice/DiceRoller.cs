using System;

namespace FGJ.LiarDice
{
    public interface IDiceRoller
    {
        /// <summary>回傳 1～6 的點數。</summary>
        int Roll();
    }

    public sealed class RandomDiceRoller : IDiceRoller
    {
        private readonly Random _random;

        public RandomDiceRoller() : this(new Random()) { }

        public RandomDiceRoller(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public int Roll() => _random.Next(LiarDiceRules.MinFace, LiarDiceRules.MaxFace + 1);
    }
}
