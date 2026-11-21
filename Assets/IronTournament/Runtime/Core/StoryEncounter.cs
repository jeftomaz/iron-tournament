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

        public static void ApplyGoblinExperience(StoryRun run, CombatantState hero)
        {
            if (run.reward == ItemId.None || run.goblinExperience || hero.Id != run.hero)
                throw new InvalidOperationException("Experience unavailable.");
            BattleExperience.Apply(hero, CombatantId.Goblin);
            run.goblinExperience = true;
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
                var state = new CombatantState(hero, stats);
                if (run.goblinExperience) BattleExperience.Apply(state, CombatantId.Goblin);
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
                    run.skeletonReward = id;
                    BattleExperience.Apply(hero, CombatantId.Skeleton);
                    run.skeletonExperience = true;
                    ItemRules.Apply(item, hero); run.skeletonHealth = hero.CurrentHealth;
                    run.skeletonScrolls = hero.HopeScrolls; run.skeletonHorns = hero.GuardianHorns;
                    run.skeletonGuardianArmed = hero.IsGuardianArmed; run.skeletonAttack = hero.Stats.Attack;
                    run.skeletonDefense = hero.Stats.Defense; run.page = StoryPage.Castle; return;
                }
            throw new ArgumentException("Reward is not in the offer.");
        }

        public static IBattle CreateKnight(StoryRun run, CombatantConfiguration hero,
            EncounterConfiguration goblin, EncounterConfiguration skeleton, EncounterConfiguration knight)
        {
            run.Validate();
            if ((run.page != StoryPage.Courtyard && run.page != StoryPage.KnightBattle) || run.knightDefeated ||
                knight == null || knight.Opponent.Id != CombatantId.Knight)
                throw new ArgumentException("Story encounter mismatch.");
            var player = RestoreKnightEntry(run, hero, goblin, skeleton);
            var random = new SeededRandomSource(run.seed ^ 0x39f6214b);
            var opponent = EncounterSetup.CreateOpponent(knight.Opponent, knight.EnemyStatVariancePercent, false, random);
            return new Battle(new BattleState(player, opponent), random);
        }

        private static CombatantState RestoreKnightEntry(StoryRun run, CombatantConfiguration hero,
            EncounterConfiguration goblin, EncounterConfiguration skeleton)
        {
            if (hero == null || hero.Id != run.hero ||
                goblin == null || goblin.Opponent.Id != CombatantId.Goblin ||
                skeleton == null || skeleton.Opponent.Id != CombatantId.Skeleton ||
                run.skeletonReward == ItemId.None)
                throw new ArgumentException("Story encounter mismatch.");

            var player = RestoreRewardHero(run, hero, goblin, 0);
            if (run.equipment) player.EquipStoryEquipment(2);
            if (run.skeletonExperience) BattleExperience.Apply(player, CombatantId.Skeleton);
            var found = false;
            foreach (var item in SkeletonRewards(run, skeleton))
                if (item.Id == run.skeletonReward) { ItemRules.Apply(item, player); found = true; break; }
            if (!found || player.Stats.Attack != run.skeletonAttack || player.Stats.Defense != run.skeletonDefense ||
                run.skeletonHealth <= 0 || run.skeletonHealth > player.Stats.MaximumHealth ||
                run.skeletonScrolls > player.HopeScrolls ||
                run.skeletonHorns + (run.skeletonGuardianArmed ? 1 : 0) > player.GuardianHorns)
                throw new ArgumentException("Invalid skeleton reward checkpoint.");
            player.TakeDamage(player.CurrentHealth - run.skeletonHealth);
            while (player.HopeScrolls > run.skeletonScrolls) player.ConsumeHopeScroll();
            while (player.GuardianHorns > run.skeletonHorns) { player.ArmGuardian(); player.DisarmGuardian(); }
            if (run.skeletonGuardianArmed) { player.AddGuardianHorn(); player.ArmGuardian(); }
            if (run.mirrorShield) player.EquipMirrorShield();
            if (run.amethystAmulet) player.EquipAmethystAmulet();
            player.PrepareForEncounter();
            return player;
        }

        public static IReadOnlyList<ItemConfiguration> LaterRewards(StoryRun run, EncounterConfiguration encounter)
        {
            if (encounter == null || (encounter.Opponent.Id != CombatantId.Knight && encounter.Opponent.Id != CombatantId.Werewolf))
                throw new ArgumentException("Invalid encounter.");
            var pool = ItemRules.Eligible(encounter.DropPool, run.hero);
            pool.RemoveAll(item => item.Id == ItemId.FlameCloak || item.Id == ItemId.MirrorShield || item.Id == ItemId.AmethystAmulet);
            var salt = encounter.Opponent.Id == CombatantId.Knight ? 0x137c28bd : 0x4a7132c9;
            return DropTable.Roll(pool, 2, new SeededRandomSource(run.seed ^ salt));
        }

        public static void RecordLaterVictory(StoryRun run, IBattle battle)
        {
            run.Validate();
            if (battle == null || battle.State.Phase != BattlePhase.Victory || battle.State.Hero.Id != run.hero)
                throw new ArgumentException("Victory is unavailable.");
            var player = battle.State.Hero;
            if (run.page == StoryPage.KnightBattle && battle.State.Opponent.Id == CombatantId.Knight && !run.knightDefeated)
            {
                run.knightDefeated = true; run.knightHealth = player.CurrentHealth; run.knightScrolls = player.HopeScrolls;
                run.knightHorns = player.GuardianHorns; run.knightGuardianArmed = player.IsGuardianArmed;
                run.page = StoryPage.KnightReward;
            }
            else if (run.page == StoryPage.WerewolfBattle && battle.State.Opponent.Id == CombatantId.Werewolf && !run.werewolfDefeated)
            {
                run.werewolfDefeated = true; run.werewolfHealth = player.CurrentHealth; run.werewolfScrolls = player.HopeScrolls;
                run.werewolfHorns = player.GuardianHorns; run.werewolfGuardianArmed = player.IsGuardianArmed;
                run.page = StoryPage.WerewolfReward;
            }
            else throw new InvalidOperationException("Victory does not match the story.");
            run.Validate();
        }

        private static void RestoreResources(CombatantState player, int health, int scrolls, int horns, bool armed)
        {
            var availableHorns = player.GuardianHorns + (player.IsGuardianArmed ? 1 : 0);
            if (health <= 0 || health > player.Stats.MaximumHealth || scrolls > player.HopeScrolls || horns + (armed ? 1 : 0) > availableHorns)
                throw new ArgumentException("Invalid encounter checkpoint.");
            player.Heal(int.MaxValue); player.TakeDamage(player.CurrentHealth - health);
            while (player.HopeScrolls > scrolls) player.ConsumeHopeScroll();
            if (player.IsGuardianArmed) { player.DisarmGuardian(); player.AddGuardianHorn(); }
            while (player.GuardianHorns > horns + (armed ? 1 : 0)) { player.ArmGuardian(); player.DisarmGuardian(); }
            if (armed) player.ArmGuardian();
            player.PrepareForEncounter();
        }

        private static ItemConfiguration SelectedLaterReward(StoryRun run, EncounterConfiguration encounter, ItemId id)
        {
            foreach (var item in LaterRewards(run, encounter)) if (item.Id == id) return item;
            throw new ArgumentException("Reward is not in the offer.");
        }

        public static CombatantState RestoreKnightVictory(StoryRun run, CombatantConfiguration hero,
            EncounterConfiguration goblin, EncounterConfiguration skeleton, EncounterConfiguration knight)
        {
            run.Validate();
            if (!run.knightDefeated || knight == null || knight.Opponent.Id != CombatantId.Knight)
                throw new ArgumentException("Invalid knight checkpoint.");
            var player = RestoreKnightEntry(run, hero, goblin, skeleton);
            if (run.knightReward != ItemId.None)
            {
                BattleExperience.Apply(player, CombatantId.Knight);
                ItemRules.Apply(SelectedLaterReward(run, knight, run.knightReward), player);
                if (player.Stats.Attack != run.knightAttack || player.Stats.Defense != run.knightDefense)
                    throw new ArgumentException("Invalid knight attributes.");
            }
            RestoreResources(player, run.knightHealth, run.knightScrolls, run.knightHorns, run.knightGuardianArmed);
            return player;
        }

        public static IBattle CreateWerewolf(StoryRun run, CombatantConfiguration hero, EncounterConfiguration goblin,
            EncounterConfiguration skeleton, EncounterConfiguration knight, EncounterConfiguration werewolf)
        {
            run.Validate();
            if (run.page != StoryPage.WerewolfBattle || !run.werewolfAppeared || run.werewolfDefeated ||
                run.knightReward == ItemId.None || werewolf == null || werewolf.Opponent.Id != CombatantId.Werewolf)
                throw new ArgumentException("Story encounter mismatch.");
            var player = RestoreKnightVictory(run, hero, goblin, skeleton, knight);
            var random = new SeededRandomSource(run.seed ^ 0x4c2e7791);
            var opponent = EncounterSetup.CreateOpponent(werewolf.Opponent, werewolf.EnemyStatVariancePercent, false, random);
            return new Battle(new BattleState(player, opponent), random);
        }

        public static CombatantState RestoreWerewolfVictory(StoryRun run, CombatantConfiguration hero,
            EncounterConfiguration goblin, EncounterConfiguration skeleton, EncounterConfiguration knight, EncounterConfiguration werewolf)
        {
            run.Validate();
            if (!run.werewolfDefeated || run.knightReward == ItemId.None || werewolf == null || werewolf.Opponent.Id != CombatantId.Werewolf)
                throw new ArgumentException("Invalid werewolf checkpoint.");
            var player = RestoreKnightVictory(run, hero, goblin, skeleton, knight);
            if (run.werewolfReward != ItemId.None)
            {
                BattleExperience.Apply(player, CombatantId.Werewolf);
                ItemRules.Apply(SelectedLaterReward(run, werewolf, run.werewolfReward), player);
                if (player.Stats.Attack != run.werewolfAttack || player.Stats.Defense != run.werewolfDefense)
                    throw new ArgumentException("Invalid werewolf attributes.");
            }
            RestoreResources(player, run.werewolfHealth, run.werewolfScrolls, run.werewolfHorns, run.werewolfGuardianArmed);
            return player;
        }

        public static void ChooseLaterReward(StoryRun run, EncounterConfiguration encounter, ItemId id, CombatantState player)
        {
            run.Validate();
            if (player == null || player.Id != run.hero || encounter == null) throw new ArgumentException("Invalid reward checkpoint.");
            var knight = run.page == StoryPage.KnightReward && encounter.Opponent.Id == CombatantId.Knight && run.knightReward == ItemId.None;
            var werewolf = run.page == StoryPage.WerewolfReward && encounter.Opponent.Id == CombatantId.Werewolf && run.werewolfReward == ItemId.None;
            if (!knight && !werewolf) throw new InvalidOperationException("Reward unavailable.");
            if (player.CurrentHealth != (knight ? run.knightHealth : run.werewolfHealth) ||
                player.HopeScrolls != (knight ? run.knightScrolls : run.werewolfScrolls) ||
                player.GuardianHorns != (knight ? run.knightHorns : run.werewolfHorns) ||
                player.IsGuardianArmed != (knight ? run.knightGuardianArmed : run.werewolfGuardianArmed) ||
                player.Stats.Attack != (knight ? run.skeletonAttack : run.knightAttack) ||
                player.Stats.Defense != (knight ? run.skeletonDefense + (run.mirrorShield ? 10 : 0) : run.knightDefense) ||
                player.HasMirrorShield != run.mirrorShield || player.HasAmethystAmulet != run.amethystAmulet)
                throw new ArgumentException("Invalid reward checkpoint.");
            var item = SelectedLaterReward(run, encounter, id);
            BattleExperience.Apply(player, encounter.Opponent.Id); ItemRules.Apply(item, player);
            if (knight)
            {
                run.knightReward = id; run.knightHealth = player.CurrentHealth; run.knightScrolls = player.HopeScrolls;
                run.knightHorns = player.GuardianHorns; run.knightGuardianArmed = player.IsGuardianArmed;
                run.knightAttack = player.Stats.Attack; run.knightDefense = player.Stats.Defense; run.page = StoryPage.Anteroom;
            }
            else
            {
                run.werewolfReward = id; run.werewolfHealth = player.CurrentHealth; run.werewolfScrolls = player.HopeScrolls;
                run.werewolfHorns = player.GuardianHorns; run.werewolfGuardianArmed = player.IsGuardianArmed;
                run.werewolfAttack = player.Stats.Attack; run.werewolfDefense = player.Stats.Defense; run.page = StoryPage.CarnalHall;
            }
            run.Validate();
        }
    }
}
