using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IronTournament.Core
{
    public sealed class ProgressData
    {
        private readonly ReadOnlyCollection<HeroSnapshot> clearedHeroes;

        public ProgressData()
            : this(new List<HeroSnapshot>())
        {
        }

        public ProgressData(IList<HeroSnapshot> clearedHeroes)
        {
            if (clearedHeroes == null)
            {
                throw new ArgumentNullException(nameof(clearedHeroes));
            }

            var classes = new HashSet<CombatantId>();
            for (var index = 0; index < clearedHeroes.Count; index++)
            {
                var snapshot = clearedHeroes[index];
                if (snapshot == null)
                {
                    throw new ArgumentException("A snapshot is required.", nameof(clearedHeroes));
                }

                if (!classes.Add(snapshot.HeroClass))
                {
                    throw new ArgumentException("Each class is recorded once.", nameof(clearedHeroes));
                }
            }

            this.clearedHeroes = new ReadOnlyCollection<HeroSnapshot>(new List<HeroSnapshot>(clearedHeroes));
        }

        public IReadOnlyList<HeroSnapshot> ClearedHeroes => clearedHeroes;

        public bool IsEnemyModeUnlocked => IsCleared(CombatantId.Warrior) && IsCleared(CombatantId.Mage);

        public bool IsCleared(CombatantId heroClass)
        {
            return FinalSnapshot(heroClass) != null;
        }

        public HeroSnapshot FinalSnapshot(CombatantId heroClass)
        {
            for (var index = 0; index < clearedHeroes.Count; index++)
            {
                if (clearedHeroes[index].HeroClass == heroClass)
                {
                    return clearedHeroes[index];
                }
            }

            return null;
        }

        public ProgressData WithCompletedCampaign(CampaignRun run)
        {
            if (run == null)
            {
                throw new ArgumentNullException(nameof(run));
            }

            if (run.FinalSnapshot == null)
            {
                throw new ArgumentException("Only a completed campaign can be recorded.", nameof(run));
            }

            var updated = new List<HeroSnapshot>();
            for (var index = 0; index < clearedHeroes.Count; index++)
            {
                if (clearedHeroes[index].HeroClass != run.FinalSnapshot.HeroClass)
                {
                    updated.Add(clearedHeroes[index]);
                }
            }

            updated.Add(run.FinalSnapshot);
            return new ProgressData(updated);
        }
    }
}
