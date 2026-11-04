using System;
using System.Collections.Generic;

namespace IronTournament.Core
{
    public static class StoryEncounter
    {
        public static IBattle Create(StoryRun run, CombatantConfiguration hero, EncounterConfiguration encounter)
        {
            run.Validate();
            if (hero.Id != run.hero || encounter.Opponent.Id != CombatantId.Goblin) throw new ArgumentException("Story encounter mismatch.");
            var random = new SeededRandomSource(run.seed);
            var opponent = EncounterSetup.CreateOpponent(encounter.Opponent, encounter.EnemyStatVariancePercent, false, random);
            var player = new CombatantState(hero, hero.BaseStats);
            if (run.equipment) player.EquipStoryEquipment(0);
            return new Battle(new BattleState(player, opponent), random);
        }

        public static IReadOnlyList<ItemConfiguration> Rewards(StoryRun run, EncounterConfiguration encounter)
        {
            var pool = ItemRules.Eligible(encounter.DropPool, run.hero);
            pool.RemoveAll(item => item.Id == ItemId.FlameCloak);
            return DropTable.Roll(pool, 2, new SeededRandomSource(run.seed ^ 0x5f3759df));
        }

        public static void ChooseReward(StoryRun run, EncounterConfiguration encounter, ItemId id)
        {
            if (run.page != StoryPage.Reward || run.reward != ItemId.None) throw new InvalidOperationException("Reward unavailable.");
            foreach (var item in Rewards(run, encounter))
                if (item.Id == id) { run.reward = id; run.page = StoryPage.Farewell; run.dialogue = 0; return; }
            throw new ArgumentException("Reward is not in the offer.");
        }

        public static void ApplyReward(StoryRun run, EncounterConfiguration encounter, CombatantState hero)
        {
            foreach (var item in Rewards(run, encounter))
                if (item.Id == run.reward) { ItemRules.Apply(item, hero); return; }
            throw new ArgumentException("Invalid selected reward.");
        }

        public static CombatantState RestoreVictoryHero(StoryRun run, CombatantConfiguration hero)
        {
            if (run.victoryHealth <= 0 || run.victoryHealth > hero.BaseStats.MaximumHealth)
                throw new ArgumentException("Invalid victory checkpoint health.");
            var state = new CombatantState(hero, hero.BaseStats);
            state.TakeDamage(hero.BaseStats.MaximumHealth - run.victoryHealth);
            if (run.equipment) state.EquipStoryEquipment(0);
            return state;
        }
    }
}
