using System.Collections.Generic;

namespace IronTournament.Core.Tests
{
    internal static class TestContent
    {
        private static readonly CombatantStats[] CanonicalEnemyStats =
        {
            new CombatantStats(45, 15, 3),
            new CombatantStats(62, 20, 5),
            new CombatantStats(80, 26, 8),
            new CombatantStats(95, 30, 10),
            new CombatantStats(110, 34, 12),
            new CombatantStats(125, 38, 14),
            new CombatantStats(145, 44, 16)
        };

        public static CombatantConfiguration Warrior()
        {
            return Hero(
                CombatantId.Warrior,
                new CombatantStats(120, 30, 23),
                AbilityId.BasicAttack,
                AbilityId.Guard,
                AbilityId.UseHopeScroll);
        }

        public static CombatantConfiguration Mage()
        {
            return Hero(
                CombatantId.Mage,
                new CombatantStats(110, 30, 5),
                AbilityId.BasicAttack,
                AbilityId.RevertTurn,
                AbilityId.RevertBattle,
                AbilityId.ArmGuardian);
        }

        public static CombatantConfiguration Goblin()
        {
            return Enemy(CombatantId.Goblin, 45, 15, 3);
        }

        public static CombatantConfiguration Hero(CombatantId id, CombatantStats stats, params AbilityId[] abilities)
        {
            return Combatant(id, CombatantSide.Player, stats, abilities);
        }

        public static CombatantConfiguration Enemy(CombatantId id, int health, int attack, int defense)
        {
            return Combatant(
                id,
                CombatantSide.Enemy,
                new CombatantStats(health, attack, defense),
                new[] { AbilityId.BasicAttack });
        }

        public static CombatantConfiguration WeakEnemy(CombatantId id)
        {
            return Enemy(id, 1, 1, 0);
        }

        public static ItemConfiguration Item(ItemId id, int dropWeight, params ItemModifier[] modifiers)
        {
            return new ItemConfiguration(id, id.ToString(), dropWeight, modifiers);
        }

        public static CampaignConfiguration Campaign(params CombatantConfiguration[] overrides)
        {
            return new CampaignConfiguration(CanonicalEncounters(false, overrides));
        }

        public static CampaignConfiguration WeakCampaign(params CombatantConfiguration[] overrides)
        {
            return new CampaignConfiguration(CanonicalEncounters(true, overrides));
        }

        public static CampaignConfiguration CampaignWithDrops(
            IList<ItemConfiguration> dropPool,
            params CombatantConfiguration[] overrides)
        {
            return new CampaignConfiguration(CanonicalEncounters(dropPool, false, overrides));
        }

        public static List<EncounterConfiguration> CanonicalEncounters(bool weak, params CombatantConfiguration[] overrides)
        {
            return CanonicalEncounters(new List<ItemConfiguration>(), weak, overrides);
        }

        // Cada encontro vencido sem rolagens de ataque consome só as três rolagens de variação (0).
        public static int[] RollsAfter(int skippedEncounters, params int[] rolls)
        {
            var values = new int[skippedEncounters * 3 + rolls.Length];
            rolls.CopyTo(values, skippedEncounters * 3);
            return values;
        }

        public static void WinWithOneHit(CampaignRun run, int encounters)
        {
            for (var index = 0; index < encounters; index++)
            {
                run.CurrentBattle.Submit(AbilityId.BasicAttack);
                run.ConcludeEncounter();
            }
        }

        public static Battle StartBattle(
            CombatantConfiguration hero,
            CombatantConfiguration opponent,
            IRandomSource random)
        {
            var state = new BattleState(
                new CombatantState(hero, hero.BaseStats),
                new CombatantState(opponent, opponent.BaseStats));
            return new Battle(state, random);
        }

        private static List<EncounterConfiguration> CanonicalEncounters(
            IList<ItemConfiguration> dropPool,
            bool weak,
            CombatantConfiguration[] overrides)
        {
            var encounters = new List<EncounterConfiguration>();
            for (var index = 0; index < CampaignConfiguration.CanonicalOrder.Count; index++)
            {
                var id = CampaignConfiguration.CanonicalOrder[index];
                var stats = CanonicalEnemyStats[index];
                var opponent = weak ? WeakEnemy(id) : Enemy(id, stats.MaximumHealth, stats.Attack, stats.Defense);
                foreach (var replacement in overrides)
                {
                    if (replacement.Id == id)
                    {
                        opponent = replacement;
                    }
                }

                encounters.Add(new EncounterConfiguration(
                    opponent,
                    EncounterConfiguration.RequiredEnemyStatVariancePercent,
                    dropPool));
            }

            return encounters;
        }

        private static CombatantConfiguration Combatant(
            CombatantId id,
            CombatantSide side,
            CombatantStats stats,
            AbilityId[] abilities)
        {
            var configurations = new List<AbilityConfiguration>();
            foreach (var ability in abilities)
            {
                var consumesTurn = ability != AbilityId.RevertTurn && ability != AbilityId.RevertBattle;
                var target = ability == AbilityId.BasicAttack ? AbilityTarget.Opponent : AbilityTarget.Self;
                configurations.Add(new AbilityConfiguration(ability, ability.ToString(), target, consumesTurn));
            }

            return new CombatantConfiguration(id, side, id.ToString(), stats, configurations);
        }
    }
}
