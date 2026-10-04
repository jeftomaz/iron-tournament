using System;
using System.Collections.Generic;

namespace IronTournament.Core.Tests
{
    internal sealed class ScriptedRandomSource : IRandomSource
    {
        private readonly Queue<int> values;

        public ScriptedRandomSource(params int[] values)
        {
            this.values = new Queue<int>(values);
        }

        public int Remaining => values.Count;

        public int Next(int minInclusive, int maxExclusive)
        {
            if (values.Count == 0)
            {
                throw new InvalidOperationException("No scripted random value left.");
            }

            var value = values.Dequeue();
            if (value < minInclusive || value >= maxExclusive)
            {
                throw new InvalidOperationException(
                    $"Scripted value {value} is outside [{minInclusive}, {maxExclusive}).");
            }

            return value;
        }
    }
}
