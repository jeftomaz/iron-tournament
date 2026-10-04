using System;

namespace IronTournament.Core.Tests
{
    internal sealed class NeutralRandomSource : IRandomSource
    {
        public int Next(int minInclusive, int maxExclusive)
        {
            return Math.Min(Math.Max(0, minInclusive), maxExclusive - 1);
        }
    }
}
