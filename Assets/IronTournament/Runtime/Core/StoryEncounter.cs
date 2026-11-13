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

        public static IBattle CreateSkeleton(StoryRun run, CombatantConfiguration hero,
            EncounterConfiguration goblin, EncounterConfiguration skeleton)
        {
            run.Validate();
            if (hero.Id != run.hero || goblin.Opponent.Id != CombatantId.Goblin ||
                skeleton.Opponent.Id != CombatantId.Skeleton || !run.skeletonAppeared || run.skeletonDefeated)
                throw new ArgumentException("Story encounter mismatch.");
            var player = RestoreSkeletonHero(run, hero, goblin, false);
            var random = new SeededRandomSource(run.seed ^ 0x1f6d932b);
            var opponent = EncounterSetup.CreateOpponent(skeleton.Opponent, skeleton.EnemyStatVariancePercent, false, random);
            return new Battle(new BattleState(player, opponent), random);
        }

        public static CombatantState RestoreSkeletonHero(StoryRun run, CombatantConfiguration hero,
            EncounterConfiguration goblin, bool afterBattle)
        {
            run.Validate();
            if (hero == null || hero.Id != run.hero || goblin == null || goblin.Opponent.Id != CombatantId.Goblin ||
                !run.skeletonAppeared || (afterBattle != run.skeletonDefeated))
                throw new ArgumentException("Invalid skeleton checkpoint.");
            var health = afterBattle ? run.skeletonHealth : run.victoryHealth;
            var state = RestoreRewardHero(run, hero, goblin, health);
            if (run.equipment) state.EquipStoryEquipment(1);
            if (health <= 0 || health > state.Stats.MaximumHealth) throw new ArgumentException("Invalid checkpoint health.");
            state.TakeDamage(state.CurrentHealth - health);
            if (afterBattle)
            {
                if (run.skeletonScrolls > state.HopeScrolls || run.skeletonHorns > state.GuardianHorns ||
                    (run.skeletonGuardianArmed && state.GuardianHorns == 0))
                    throw new ArgumentException("Invalid item checkpoint.");
                if (state.HopeScrolls > run.skeletonScrolls) state.ConsumeHopeScroll();
                if (state.GuardianHorns > run.skeletonHorns)
                { state.ArmGuardian(); if (!run.skeletonGuardianArmed) state.DisarmGuardian(); }
            }
            return state;
        }

        private static CombatantState RestoreRewardHero(StoryRun run, CombatantConfiguration hero,
            EncounterConfiguration goblin, int health)
        {
            var legacy = hero.Id == CombatantId.Warrior
                ? new CombatantStats(120, 30, 23) : new CombatantStats(110, 30, 5);
            foreach (var stats in new[] { hero.BaseStats, legacy })
            {
                var restored = run.goblinExperience
                    ? new CombatantStats(stats.MaximumHealth, stats.Attack + 2, stats.Defense + 1) : stats;
                var state = new CombatantState(hero, restored);
                ApplyReward(run, goblin, state);
                if (state.Stats.Attack == run.rewardAttack && state.Stats.Defense == run.rewardDefense &&
                    health <= state.Stats.MaximumHealth) return state;
            }
            throw new ArgumentException("Invalid reward checkpoint.");
        }

        public static IReadOnlyList<ItemConfiguration> SkeletonRewards(StoryRun run, EncounterConfiguration encounter)
        {
            if (encounter.Opponent.Id != CombatantId.Skeleton) throw new ArgumentException("Invalid encounter.");
            var pool = ItemRules.Eligible(encounter.DropPool, run.hero);
            pool.RemoveAll(item => item.Id == ItemId.FlameCloak);
            return DropTable.Roll(pool, 2, new SeededRandomSource(run.seed ^ 0x55b47119));
        }

        public static void ChooseSkeletonReward(StoryRun run, EncounterConfiguration encounter, ItemId id, CombatantState hero)
        {
            if (run.page != StoryPage.SkeletonReward || !run.skeletonDefeated || run.skeletonReward != ItemId.None || hero.Id != run.hero)
                throw new InvalidOperationException("Reward unavailable.");
            foreach (var item in SkeletonRewards(run, encounter))
                if (item.Id == id)
                {
                    ItemRules.Apply(item, hero); run.skeletonReward = id; run.skeletonHealth = hero.CurrentHealth;
                    run.skeletonScrolls = hero.HopeScrolls; run.skeletonHorns = hero.GuardianHorns;
                    run.skeletonGuardianArmed = hero.IsGuardianArmed; run.skeletonAttack = hero.Stats.Attack;
                    run.skeletonDefense = hero.Stats.Defense; run.page = StoryPage.Castle; return;
                }
            throw new ArgumentException("Reward is not in the offer.");
        }
    }
}
