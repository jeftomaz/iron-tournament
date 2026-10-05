using System;
using System.Collections.Generic;

namespace IronTournament.Core
{
    internal static class EnemyModeSetup
    {
        public const int PreBossDropOfferSize = 3;

        private const string FuryDisplayName = "Fúria";
        private const string AttackDisplayName = "Atacar";

        private static readonly CombatantId[][] Tiers =
        {
            new[] { CombatantId.Goblin, CombatantId.Skeleton },
            new[] { CombatantId.Knight, CombatantId.Werewolf },
            new[] { CombatantId.Vampire, CombatantId.Necromancer }
        };

        public static CombatantConfiguration Character(CombatantId character, CampaignConfiguration campaign)
        {
            if (!Roster(campaign).TryGetValue(character, out var chosen))
            {
                throw new ArgumentException("The character is not in the roster.", nameof(character));
            }

            return AsPlayer(chosen);
        }

        public static List<EncounterStage> Stages(
            CombatantId character,
            CampaignConfiguration campaign,
            ProgressData progress,
            IRandomSource random)
        {
            var roster = Roster(campaign);
            var preBossPool = ItemRules.Eligible(DistinctDrops(campaign), character);
            var stages = new List<EncounterStage>();
            for (var tier = 0; tier < Tiers.Length; tier++)
            {
                var isLastTier = tier == Tiers.Length - 1;
                stages.Add(new EncounterStage(
                    PickTierOpponent(Tiers[tier], character, roster, random),
                    EncounterConfiguration.RequiredEnemyStatVariancePercent,
                    false,
                    isLastTier ? preBossPool : new List<ItemConfiguration>(),
                    isLastTier ? PreBossDropOfferSize : 0));
            }

            var bossClass = random.Next(0, 2) == 0 ? CombatantId.Warrior : CombatantId.Mage;
            stages.Add(new EncounterStage(
                AsBoss(progress.FinalSnapshot(bossClass)),
                0,
                false,
                new List<ItemConfiguration>(),
                0));
            return stages;
        }

        private static Dictionary<CombatantId, CombatantConfiguration> Roster(CampaignConfiguration campaign)
        {
            var roster = new Dictionary<CombatantId, CombatantConfiguration>();
            foreach (var encounter in campaign.Encounters)
            {
                roster[encounter.Opponent.Id] = encounter.Opponent;
            }

            return roster;
        }

        private static List<ItemConfiguration> DistinctDrops(CampaignConfiguration campaign)
        {
            var seen = new HashSet<ItemId>();
            var drops = new List<ItemConfiguration>();
            foreach (var encounter in campaign.Encounters)
            {
                foreach (var item in encounter.DropPool)
                {
                    if (seen.Add(item.Id))
                    {
                        drops.Add(item);
                    }
                }
            }

            return drops;
        }

        private static CombatantConfiguration PickTierOpponent(
            CombatantId[] tier,
            CombatantId character,
            Dictionary<CombatantId, CombatantConfiguration> roster,
            IRandomSource random)
        {
            var options = new List<CombatantConfiguration>();
            foreach (var id in tier)
            {
                if (id != character && roster.TryGetValue(id, out var opponent))
                {
                    options.Add(opponent);
                }
            }

            if (options.Count == 0)
            {
                throw new ArgumentException("The roster cannot fill every tier.", nameof(roster));
            }

            return options[random.Next(0, options.Count)];
        }

        private static CombatantConfiguration AsPlayer(CombatantConfiguration enemy)
        {
            var abilities = new List<AbilityConfiguration>();
            var hasFury = false;
            foreach (var ability in enemy.Abilities)
            {
                abilities.Add(ability);
                hasFury |= ability.Id == AbilityId.Fury;
            }

            if (!hasFury)
            {
                abilities.Add(new AbilityConfiguration(AbilityId.Fury, FuryDisplayName, AbilityTarget.Opponent, true));
            }

            return new CombatantConfiguration(
                enemy.Id,
                CombatantSide.Player,
                enemy.DisplayName,
                enemy.BaseStats,
                abilities);
        }

        private static CombatantConfiguration AsBoss(HeroSnapshot snapshot)
        {
            var attack = new AbilityConfiguration(AbilityId.BasicAttack, AttackDisplayName, AbilityTarget.Opponent, true);
            return new CombatantConfiguration(
                snapshot.HeroClass,
                CombatantSide.Enemy,
                snapshot.DisplayName,
                snapshot.Stats,
                new[] { attack });
        }
    }
}
