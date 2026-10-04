using System.Collections.Generic;

namespace IronTournament.Core.Tests
{
    internal static class TestContent
    {
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
            return Combatant(id, CombatantSide.Enemy, new CombatantStats(health, attack, defense), AbilityId.BasicAttack);
        }

        public static ItemConfiguration Item(ItemId id, int dropWeight, params ItemModifier[] modifiers)
        {
            return new ItemConfiguration(id, id.ToString(), dropWeight, modifiers);
        }

        public static CampaignConfiguration Campaign(params CombatantConfiguration[] opponents)
        {
            return CampaignWithDrops(new List<ItemConfiguration>(), opponents);
        }

        public static CampaignConfiguration CampaignWithDrops(
            IList<ItemConfiguration> dropPool,
            params CombatantConfiguration[] opponents)
        {
            var encounters = new List<EncounterConfiguration>();
            foreach (var opponent in opponents)
            {
                encounters.Add(new EncounterConfiguration(
                    opponent,
                    EncounterConfiguration.RequiredEnemyStatVariancePercent,
                    dropPool));
            }

            return new CampaignConfiguration(encounters);
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
