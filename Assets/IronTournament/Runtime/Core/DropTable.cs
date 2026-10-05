using System;
using System.Collections.Generic;

namespace IronTournament.Core
{
    internal static class DropTable
    {
        public static List<ItemConfiguration> Roll(IList<ItemConfiguration> pool, int count, IRandomSource random)
        {
            var remaining = new List<ItemConfiguration>(pool);
            var offer = new List<ItemConfiguration>();
            while (offer.Count < count && remaining.Count > 0)
            {
                var totalWeight = 0L;
                foreach (var item in remaining)
                {
                    totalWeight += item.DropWeight;
                }

                if (totalWeight > int.MaxValue)
                {
                    throw new InvalidOperationException("Drop weights are too large.");
                }

                var roll = random.Next(0, (int)totalWeight);
                var index = 0;
                while (roll >= remaining[index].DropWeight)
                {
                    roll -= remaining[index].DropWeight;
                    index++;
                }

                offer.Add(remaining[index]);
                remaining.RemoveAt(index);
            }

            return offer;
        }
    }
}
