using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace IronTournament.Core.Tests
{
    public sealed class RegressionTests
    {
        private const int SeedCount = 40;
        private const int StepLimit = 5000;

        [Test]
        public void CanonicalCampaignsFinishWithValidStateForEverySeed()
        {
            for (var seed = 1; seed <= SeedCount; seed++)
            {
                foreach (var hero in new[] { TestContent.Warrior(), TestContent.Mage() })
                {
                    var run = Play(new CampaignRun(hero, Canonical(), new SeededRandomSource(seed)));

                    Assert.That(run.Phase, Is.EqualTo(CampaignPhase.Completed).Or.EqualTo(CampaignPhase.Failed));
                    Assert.That(run.FinalSnapshot != null, Is.EqualTo(run.Phase == CampaignPhase.Completed));
                    Assert.That(run.ObtainedItems.Count, Is.LessThanOrEqualTo(run.EncounterCount - 1));
                }
            }
        }

        [Test]
        public void CanonicalCampaignIsDeterministicForTheSameSeed()
        {
            for (var seed = 1; seed <= SeedCount; seed++)
            {
                foreach (var hero in new[] { TestContent.Warrior(), TestContent.Mage() })
                {
                    var first = Play(new CampaignRun(hero, Canonical(), new SeededRandomSource(seed)));
                    var second = Play(new CampaignRun(hero, Canonical(), new SeededRandomSource(seed)));

                    Assert.That(Summary(second), Is.EqualTo(Summary(first)));
                }
            }
        }

        [Test]
        public void EnemyModeFinishesForEveryCharacter()
        {
            var progress = new ProgressData(new[]
            {
                new HeroSnapshot(CombatantId.Warrior, "Guerreiro", new CombatantStats(120, 45, 31)),
                new HeroSnapshot(CombatantId.Mage, "Mago", new CombatantStats(110, 45, 10))
            });
            var characters = new[]
            {
                CombatantId.Goblin,
                CombatantId.Skeleton,
                CombatantId.Knight,
                CombatantId.Werewolf,
                CombatantId.Vampire,
                CombatantId.Necromancer,
                CombatantId.DemonKing
            };

            for (var seed = 1; seed <= SeedCount; seed++)
            {
                foreach (var character in characters)
                {
                    var run = Play(CampaignRun.StartEnemyMode(character, Canonical(), progress, new SeededRandomSource(seed)));

                    Assert.That(run.Phase, Is.EqualTo(CampaignPhase.Completed).Or.EqualTo(CampaignPhase.Failed));
                    Assert.That(run.FinalSnapshot, Is.Null);
                    Assert.That(run.Opponents.Count, Is.EqualTo(4));
                    Assert.That(run.Opponents, Has.No.Member(character));
                }
            }
        }

        private static CampaignRun Play(CampaignRun run)
        {
            for (var step = 0; step < StepLimit; step++)
            {
                switch (run.Phase)
                {
                    case CampaignPhase.DropChoice:
                        Assert.That(run.ChooseDrop(run.DropOffer[0].Id), Is.True);
                        break;
                    case CampaignPhase.Battle when run.CurrentBattle.State.IsOver:
                        Assert.That(run.ConcludeEncounter(), Is.True);
                        break;
                    case CampaignPhase.Battle:
                        var battle = run.CurrentBattle;
                        AssertSubmit(battle, Choose(battle));
                        break;
                    default:
                        return run;
                }
            }

            Assert.Fail("The run did not finish within the step limit.");
            return run;
        }

        private static AbilityId Choose(IBattle battle)
        {
            var actions = battle.AvailableActions;
            var hero = battle.State.Hero;
            var preferred = new List<AbilityId> { AbilityId.Fury, AbilityId.UseHopeScroll, AbilityId.ArmGuardian };
            if (hero.IsHealthAtOrBelow(30))
            {
                preferred.Add(AbilityId.RevertTurn);
            }

            if (battle.State.Round % 3 == 0)
            {
                preferred.Add(AbilityId.Guard);
            }

            foreach (var action in preferred)
            {
                if (Contains(actions, action))
                {
                    return action;
                }
            }

            return AbilityId.BasicAttack;
        }

        private static void AssertSubmit(IBattle battle, AbilityId action)
        {
            var result = battle.Submit(action);
            var state = battle.State;

            Assert.That(result.IsAccepted, Is.True, action.ToString());
            Assert.That(result.Events.Count, Is.GreaterThan(0));
            AssertHealth(state.Hero);
            AssertHealth(state.Opponent);
            Assert.That(state.IsOver, Is.EqualTo(result.Events[result.Events.Count - 1] is BattleEndedEvent));
            if (!state.IsOver)
            {
                Assert.That(state.Hero.GuardBonus, Is.Zero);
            }
        }

        private static void AssertHealth(CombatantState combatant)
        {
            Assert.That(combatant.CurrentHealth, Is.InRange(0, combatant.Stats.MaximumHealth));
        }

        private static bool Contains(IReadOnlyList<AbilityId> actions, AbilityId action)
        {
            for (var index = 0; index < actions.Count; index++)
            {
                if (actions[index] == action)
                {
                    return true;
                }
            }

            return false;
        }

        private static string Summary(CampaignRun run)
        {
            return $"{run.Phase}|{run.EncounterIndex}|{run.Hero.CurrentHealth}|{run.Hero.Stats.Attack}|"
                + string.Join(",", run.ObtainedItems);
        }

        private static CampaignConfiguration Canonical()
        {
            var dropPool = new[]
            {
                TestContent.Item(ItemId.HealingPotion, 3),
                TestContent.Item(ItemId.AttackGem, 3, new ItemModifier(ItemModifierKind.Attack, 10)),
                TestContent.Item(ItemId.DefenseRune, 3, new ItemModifier(ItemModifierKind.Defense, 5)),
                TestContent.Item(ItemId.PowerCrystal, 2, new ItemModifier(ItemModifierKind.Attack, 15)),
                TestContent.Item(ItemId.LifeElixir, 1),
                TestContent.Item(ItemId.DivineDefense, 1, new ItemModifier(ItemModifierKind.Defense, 15)),
                TestContent.Item(ItemId.HopeScroll, 2),
                TestContent.Item(ItemId.FlameCloak, 2),
                TestContent.Item(ItemId.BrotherhoodHorn, 2)
            };
            return TestContent.CampaignWithDrops(
                dropPool,
                TestContent.Goblin(),
                TestContent.Enemy(CombatantId.Skeleton, 62, 20, 5),
                TestContent.Enemy(CombatantId.Knight, 80, 26, 8),
                TestContent.Enemy(CombatantId.Werewolf, 95, 30, 10),
                TestContent.Enemy(CombatantId.Vampire, 110, 34, 12),
                TestContent.Enemy(CombatantId.Necromancer, 125, 38, 14),
                TestContent.Enemy(CombatantId.DemonKing, 145, 44, 16));
        }
    }
}
