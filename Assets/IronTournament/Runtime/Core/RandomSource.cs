using System;

namespace IronTournament.Core
{
    public interface IRandomSource
    {
        int Next(int minInclusive, int maxExclusive);
    }

    public sealed class SeededRandomSource : IRandomSource
    {
        private readonly Random random;

        public SeededRandomSource(int seed)
        {
            random = new Random(seed);
        }

        public int Next(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            }

            return random.Next(minInclusive, maxExclusive);
        }
    }
}
