using System;
using System.Linq;
using NUnit.Framework;

namespace IronTournament.Core.Tests
{
    public sealed class StoryRunTests
    {
        private static StoryRun New(CombatantId hero = CombatantId.Warrior)
        { var run = StoryRun.Create(hero, 417); run.page = StoryPage.Village; return run; }
        private static void Visit(StoryRun run, int slot)
        { run.EnterHouse(slot); run.LeaveHouse(); }

        [Test]
        public void HouseContentsAreAPermutationStableForTheSeed()
        {
            for (var seed = 0; seed < 50; seed++)
            {
                var a = StoryRun.Create(CombatantId.Warrior, seed);
                var b = StoryRun.Create(CombatantId.Mage, seed);
                Assert.That(a.houses.OrderBy(x=>x), Is.EqualTo(new[] {0,1,2,3,4}));
                Assert.That(a.houses, Is.EqualTo(b.houses));
            }
        }
        [Test]
        public void RoomsAndRevisitsDoNotConsumeAnotherRescueOpportunity()
        {
            var run = New(); run.EnterHouse(0); run.room = true; run.room = false;
            Assert.That(run.ExploredCount, Is.Zero);
            Assert.That(run.LeaveHouse(), Is.True); run.ContinueExploration();
            Visit(run,0); Assert.That(run.ExploredCount, Is.EqualTo(1)); Assert.That(run.pendingCry, Is.False);
        }
        [TestCase(1,ChildFate.Saved)]
        [TestCase(2,ChildFate.Saved)]
        [TestCase(3,ChildFate.Dying)]
        public void InvestigatingEachCrySelectsTheExpectedFate(int visits, ChildFate expected)
        {
            var run=New();
            for(var i=0;i<visits;i++) { Visit(run,i); if(i<visits-1) run.ContinueExploration(); }
            run.BeginRescue(); Assert.That(run.Fate,Is.EqualTo(expected)); run.Validate();
        }
        [Test]
        public void DecliningTheThirdCryIsPermanentAndThereAreNoFurtherCries()
        {
            var run=New();
            for(var i=0;i<3;i++) { Visit(run,i); run.ContinueExploration(); }
            Assert.That(run.Fate,Is.EqualTo(ChildFate.Dead));
            run.EnterHouse(3); Assert.That(run.LeaveHouse(),Is.False);
            run.BeginRescue(); Assert.That(run.Fate,Is.EqualTo(ChildFate.Dead));
        }
        [TestCase(CombatantId.Warrior,0)]
        [TestCase(CombatantId.Mage,1)]
        public void EquipmentIsUniqueAndClassRestricted(CombatantId hero,int house)
        {
            var run=New(hero); run.EnterHouse(Array.IndexOf(run.houses,1-house));
            Assert.That(run.CollectEquipment(),Is.False); run.LeaveHouse(); run.ContinueExploration();
            run.EnterHouse(Array.IndexOf(run.houses,house));
            Assert.That(run.CollectEquipment(),Is.True); Assert.That(run.CollectEquipment(),Is.False);
        }
        [Test]
        public void FoodIsCollectedOnceAndCannotBeGivenToADeadChild()
        {
            var run=New(); run.EnterHouse(Array.IndexOf(run.houses,3));
            Assert.That(run.CollectFood(),Is.True); Assert.That(run.CollectFood(),Is.False);
            run.LeaveHouse(); run.BeginRescue(); run.page=StoryPage.AfterBattle;
            Assert.That(run.GiveFood(),Is.True); Assert.That(run.GiveFood(),Is.False); run.Validate();
            run=New(); run.food=run.foodCollected=true; run.visited=7; run.lastCallDeclined=true; run.page=StoryPage.AfterBattle;
            Assert.That(run.GiveFood(),Is.False); Assert.That(run.food,Is.True);
        }
        [Test]
        public void SecondCampaignRequiresAnotherCompletedHeroAndFreezesOrigin()
        {
            Assert.That(StoryRun.Create(CombatantId.Warrior,1,CombatantId.Warrior).secondCampaign,Is.False);
            var run=StoryRun.Create(CombatantId.Mage,1,CombatantId.Warrior,true,8);
            Assert.That(run.secondCampaign,Is.True); Assert.That(run.Fate,Is.EqualTo(ChildFate.Absent));
            Assert.That(run.completionOrigin,Is.EqualTo(8)); run.crystal=true;
            Assert.Throws<ArgumentException>(()=>run.Validate());
        }
        [Test]
        public void RetryPreservesOpponentEquipmentAndRewardOffer()
        {
            var run=New(CombatantId.Mage); run.equipment=true;
            var encounter=new EncounterConfiguration(TestContent.Goblin(),15,new[] {
                TestContent.Item(ItemId.FlameCloak,2),TestContent.Item(ItemId.AttackGem,3),TestContent.Item(ItemId.DefenseRune,3),TestContent.Item(ItemId.HopeScroll,2) });
            var battle=StoryEncounter.Create(run,TestContent.Mage(),encounter);
            var before=battle.State.Opponent.Stats;
            var result=battle.Submit(AbilityId.BasicAttack);
            Assert.That(result.Events.OfType<DamageDealtEvent>().Where(x=>x.Kind==DamageKind.Burn).All(x=>x.Amount==1),Is.True);
            Assert.That(result.Events.OfType<DamageDealtEvent>().Count(x=>x.Kind==DamageKind.Burn),Is.EqualTo(2));
            var retry=StoryEncounter.Create(run,TestContent.Mage(),encounter);
            Assert.That(retry.State.Opponent.Stats.MaximumHealth,Is.EqualTo(before.MaximumHealth));
            Assert.That(retry.State.Hero.CurrentHealth,Is.EqualTo(110));
            Assert.That(StoryEncounter.Rewards(run,encounter).Select(x=>x.Id),Is.EqualTo(StoryEncounter.Rewards(run,encounter).Select(x=>x.Id)));
            Assert.That(StoryEncounter.Rewards(run,encounter).Any(x=>x.Id==ItemId.FlameCloak || x.Id==ItemId.HopeScroll),Is.False);
            run.page=StoryPage.Reward; var selected=StoryEncounter.Rewards(run,encounter)[0].Id;
            StoryEncounter.ChooseReward(run,encounter,selected);
            Assert.Throws<InvalidOperationException>(()=>StoryEncounter.ChooseReward(run,encounter,selected));
        }
        [Test]
        public void InvalidCheckpointIsRejected()
        {
            var run=New(); run.houses[0]=run.houses[1]; Assert.Throws<ArgumentException>(()=>run.Validate());
            run=New(); run.visited=1024; Assert.Throws<ArgumentException>(()=>run.Validate());
            run=New(); run.page=StoryPage.House; run.activeHouse=0; run.room=true;
            Assert.Throws<ArgumentException>(()=>run.Validate());
        }

        [Test]
        public void GraveLayoutAndSurpriseAreStableAndLettersStayAdjacent()
        {
            var triggers = new System.Collections.Generic.HashSet<int>();
            for (var seed = 0; seed < 150; seed++)
            {
                var run = StoryRun.Create(CombatantId.Warrior, seed); run.page = StoryPage.ColdForest; run.PrepareGraveyard();
                run.reward = ItemId.HealingPotion; run.victoryHealth = 70; run.rewardAttack = 30; run.rewardDefense = 23;
                var copy = StoryRun.Create(CombatantId.Mage, seed); copy.page = StoryPage.ColdForest; copy.PrepareGraveyard();
                Assert.That(run.graves, Is.EqualTo(copy.graves)); Assert.That(run.skeletonTrigger, Is.EqualTo(copy.skeletonTrigger));
                Assert.That(run.graves.OrderBy(x => x), Is.EqualTo(new[] { 0, 1, 2, 3, 4, 5 }));
                var lucy = Array.IndexOf(run.graves, 0); var samuel = Array.IndexOf(run.graves, 1);
                Assert.That(Math.Abs(lucy - samuel) == 3 || (lucy / 3 == samuel / 3 && Math.Abs(lucy - samuel) == 1), Is.True);
                triggers.Add(run.skeletonTrigger); run.page = StoryPage.Graveyard;
                for (var slot = 0; slot < 6 && !run.skeletonAppeared; slot++)
                {
                    run.EnterGrave(slot);
                    if (!run.skeletonAppeared) run.page = StoryPage.Graveyard;
                }
                Assert.That(run.GravesExplored, Is.EqualTo(run.skeletonTrigger - 1));
                Assert.That(run.page, Is.EqualTo(StoryPage.SkeletonEmergence)); run.Validate();
                var interrupted = run.activeGrave; run.skeletonDefeated = true; run.skeletonHealth = 40; run.page = StoryPage.Graveyard;
                run.EnterGrave(interrupted); Assert.That(run.page, Is.EqualTo(StoryPage.Grave));
                Assert.That(run.GravesExplored, Is.EqualTo(run.skeletonTrigger)); run.Validate();
            }
            Assert.That(triggers.OrderBy(x => x), Is.EqualTo(new[] { 3, 4, 5, 6 }));
        }

        [Test]
        public void CrystalFixesOneGraveFromTheBeginningRegardlessOfOpeningOrder()
        {
            var positions = new System.Collections.Generic.HashSet<int>();
            for (var seed = 0; seed < 150; seed++)
            {
                var first = StoryRun.Create(CombatantId.Warrior, seed); first.crystal = true;
                first.page = StoryPage.ColdForest; first.PrepareGraveyard();
                first.reward = ItemId.HealingPotion; first.victoryHealth = 70; first.rewardAttack = 30;
                var last = StoryRun.Create(CombatantId.Warrior, seed); last.crystal = true;
                last.page = StoryPage.ColdForest; last.PrepareGraveyard();
                last.reward = ItemId.HealingPotion; last.victoryHealth = 70; last.rewardAttack = 30;
                Assert.That(first.skeletonGraveNumber, Is.InRange(1, 6));
                Assert.That(last.skeletonGraveNumber, Is.EqualTo(first.skeletonGraveNumber));
                Assert.That(first.graves[first.skeletonGraveNumber - 1], Is.EqualTo((int)GraveContent.Empty));
                positions.Add(first.skeletonGraveNumber);
                var target = first.skeletonGraveNumber - 1; first.page = StoryPage.Graveyard;
                first.EnterGrave(target); Assert.That(first.skeletonAppeared, Is.True);
                Assert.That(first.GravesExplored, Is.Zero); first.Validate();
                last.page = StoryPage.Graveyard;
                for (var slot = 0; slot < 6; slot++)
                {
                    if (slot == target) continue;
                    Assert.That(last.TriggersSkeleton(slot), Is.False);
                    last.EnterGrave(slot); Assert.That(last.skeletonAppeared, Is.False);
                    last.page = StoryPage.Graveyard; last.Validate();
                }
                Assert.That(last.TriggersSkeleton(target), Is.True);
                last.EnterGrave(target); Assert.That(last.skeletonAppeared, Is.True);
                Assert.That(last.GravesExplored, Is.EqualTo(5)); last.Validate();
            }
            Assert.That(positions.OrderBy(x => x), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6 }));
        }

        [Test]
        public void WithoutCrystalTheSkeletonReplacesOnlyTheTriggeredGraveContent()
        {
            var run = StoryRun.Create(CombatantId.Warrior, 45);
            run.page = StoryPage.ColdForest; run.PrepareGraveyard(); run.skeletonTrigger = 3;
            run.reward = ItemId.HealingPotion; run.victoryHealth = 70; run.rewardAttack = 30;
            var original = (int[])run.graves.Clone();
            var letter = Array.IndexOf(run.graves, (int)GraveContent.LucyLetter);
            var first = Enumerable.Range(0, 6).Where(i => i != letter).Take(2).ToArray();
            run.page = StoryPage.Graveyard;
            foreach (var slot in first) { run.EnterGrave(slot); run.page = StoryPage.Graveyard; }
            run.EnterGrave(letter);
            Assert.That(run.skeletonAppeared, Is.True);
            Assert.That(run.skeletonGraveNumber, Is.EqualTo(letter + 1));
            Assert.That(run.graves[letter], Is.EqualTo((int)GraveContent.Skeleton));
            for (var i = 0; i < 6; i++) if (i != letter) Assert.That(run.graves[i], Is.EqualTo(original[i]));
            run.Validate();
            run.skeletonDefeated = true; run.skeletonHealth = 40; run.page = StoryPage.Graveyard;
            run.EnterGrave(letter); Assert.That(run.graves[letter], Is.EqualTo((int)GraveContent.Skeleton)); run.Validate();
        }

        [TestCase(CombatantId.Warrior)]
        [TestCase(CombatantId.Mage)]
        public void SkeletonRetryPreservesGoblinRewardHealthAndEquipment(CombatantId id)
        {
            var hero = id == CombatantId.Warrior ? TestContent.Warrior() : TestContent.Mage();
            var pool = new[] { TestContent.Item(ItemId.AttackGem, 3, new ItemModifier(ItemModifierKind.Attack, 10)), TestContent.Item(ItemId.HealingPotion, 3) };
            var goblin = new EncounterConfiguration(TestContent.Goblin(), 15, pool);
            var skeleton = new EncounterConfiguration(TestContent.Enemy(CombatantId.Skeleton, 62, 20, 5), 15, pool);
            var run = StoryRun.Create(id, 417); run.page = StoryPage.ColdForest; run.PrepareGraveyard();
            run.page = StoryPage.SkeletonEmergence; run.skeletonAppeared = true; run.equipment = true;
            run.gravesVisited = (1 << (run.skeletonTrigger - 1)) - 1; run.activeGrave = run.skeletonTrigger - 1;
            run.reward = ItemId.AttackGem; run.victoryHealth = 70; run.rewardAttack = 40; run.rewardDefense = hero.BaseStats.Defense;
            var first = StoryEncounter.CreateSkeleton(run, hero, goblin, skeleton);
            var retry = StoryEncounter.CreateSkeleton(run, hero, goblin, skeleton);
            Assert.That(first.State.Hero.CurrentHealth, Is.EqualTo(70)); Assert.That(first.State.Hero.Stats.Attack, Is.EqualTo(40));
            Assert.That(retry.State.Opponent.Stats.MaximumHealth, Is.EqualTo(first.State.Opponent.Stats.MaximumHealth));
            if (id == CombatantId.Mage) Assert.That(first.State.Hero.FlameCloakDamage, Is.EqualTo(3));
            else Assert.That(first.State.Hero.CriticalChancePercent, Is.EqualTo(10));
            run.skeletonDefeated = true; run.skeletonHealth = 40; run.page = StoryPage.SkeletonReward;
            var restored = StoryEncounter.RestoreSkeletonHero(run, hero, goblin, true);
            Assert.That(restored.CurrentHealth, Is.EqualTo(40));
            var offer = StoryEncounter.SkeletonRewards(run, skeleton);
            Assert.That(offer.Select(x => x.Id).Distinct().Count(), Is.EqualTo(2));
            StoryEncounter.ChooseSkeletonReward(run, skeleton, offer[0].Id, restored);
            Assert.That(run.page, Is.EqualTo(StoryPage.Castle));
            Assert.Throws<InvalidOperationException>(() => StoryEncounter.ChooseSkeletonReward(run, skeleton, offer[1].Id, restored));
        }

        [Test]
        public void SkeletonCheckpointRestoresAnArmedGuardian()
        {
            var hero = TestContent.Mage();
            var pool = new[] { TestContent.Item(ItemId.BrotherhoodHorn, 2), TestContent.Item(ItemId.HealingPotion, 3) };
            var goblin = new EncounterConfiguration(TestContent.Goblin(), 15, pool);
            var run = StoryRun.Create(CombatantId.Mage, 11); run.page = StoryPage.ColdForest; run.PrepareGraveyard();
            run.skeletonAppeared = run.skeletonDefeated = true; run.skeletonHealth = 45; run.skeletonGuardianArmed = true;
            run.gravesVisited = (1 << (run.skeletonTrigger - 1)) - 1; run.activeGrave = run.skeletonTrigger - 1;
            run.reward = ItemId.BrotherhoodHorn; run.rewardAttack = 30; run.rewardDefense = 5; run.victoryHealth = 70;
            run.page = StoryPage.SkeletonReward;
            var restored = StoryEncounter.RestoreSkeletonHero(run, hero, goblin, true);
            Assert.That(restored.IsGuardianArmed, Is.True); Assert.That(restored.GuardianHorns, Is.Zero);
            Assert.That(restored.CurrentHealth, Is.EqualTo(45));
        }

        [Test]
        public void SkeletonDoesNotReapplyHealingToTheSavedHealth()
        {
            var hero = TestContent.Warrior();
            var pool = new[] { TestContent.Item(ItemId.HealingPotion, 3), TestContent.Item(ItemId.AttackGem, 3) };
            var goblin = new EncounterConfiguration(TestContent.Goblin(), 15, pool);
            var skeleton = new EncounterConfiguration(TestContent.Enemy(CombatantId.Skeleton, 62, 20, 5), 15, pool);
            var run = StoryRun.Create(CombatantId.Warrior, 19); run.page = StoryPage.ColdForest; run.PrepareGraveyard();
            run.reward = ItemId.HealingPotion; run.rewardAttack = 30; run.rewardDefense = 23; run.victoryHealth = 70;
            run.gravesVisited = (1 << (run.skeletonTrigger - 1)) - 1; run.activeGrave = run.skeletonTrigger - 1;
            run.skeletonAppeared = true; run.page = StoryPage.SkeletonEmergence;
            Assert.That(StoryEncounter.CreateSkeleton(run, hero, goblin, skeleton).State.Hero.CurrentHealth, Is.EqualTo(70));
        }
    }
}
