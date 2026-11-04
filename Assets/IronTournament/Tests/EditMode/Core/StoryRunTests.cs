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
        }
    }
}
