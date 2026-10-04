using System.Collections.Generic;

namespace IronTournament.Core
{
    internal sealed class EncounterStage
    {
        public EncounterStage(
            CombatantConfiguration opponent,
            int variancePercent,
            bool canRage,
            IList<ItemConfiguration> dropPool,
            int dropOfferSize)
        {
            Opponent = opponent;
            VariancePercent = variancePercent;
            CanRage = canRage;
            DropPool = dropPool;
            DropOfferSize = dropOfferSize;
        }

        public CombatantConfiguration Opponent { get; }

        public int VariancePercent { get; }

        public bool CanRage { get; }

        public IList<ItemConfiguration> DropPool { get; }

        public int DropOfferSize { get; }
    }
}
