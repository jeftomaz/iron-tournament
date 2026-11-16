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

        private static StoryRun BridgeReady(CombatantId hero = CombatantId.Warrior, int seed = 417)
        {
            var run = StoryRun.Create(hero, seed); run.page = StoryPage.ColdForest; run.PrepareGraveyard();
            run.reward = ItemId.HealingPotion; run.victoryHealth = 70; run.rewardAttack = 30;
            run.skeletonTrigger = 4; run.gravesVisited = 7; run.activeGrave = 3;
            run.skeletonAppeared = run.skeletonDefeated = true; run.skeletonHealth = 40;
            run.skeletonReward = ItemId.AttackGem; run.page = StoryPage.SkeletonFinished;
            run.EnterBridge(); return run;
        }

        [Test]
        public void BridgeCagesAreAStablePermutation()
        {
            for (var seed = 0; seed < 40; seed++)
            {
                var a = BridgeReady(CombatantId.Warrior, seed);
                var b = BridgeReady(CombatantId.Mage, seed);
                Assert.That(a.cages.OrderBy(x => x), Is.EqualTo(new[] { 0, 1, 2, 3, 4, 5 }));
                Assert.That(a.cages, Is.EqualTo(b.cages)); a.Validate();
            }
        }

        [TestCase(2, 5)]
        [TestCase(3, 15)]
        [TestCase(4, 25)]
        [TestCase(5, 35)]
        [TestCase(6, 50)]
        public void BridgeFallUsesTheChanceForEachNewCage(int cageNumber, int chance)
        {
            foreach (var fall in new[] { false, true })
            {
                var run = BridgeReady();
                for (var slot = 0; slot < cageNumber - 1; slot++)
                { run.EnterCage(slot, new ScriptedRandomSource(99)); run.LeaveCage(); }
                run.EnterCage(cageNumber - 1, new ScriptedRandomSource(fall ? chance - 1 : chance));
                Assert.That(run.page, Is.EqualTo(fall ? StoryPage.BridgeFall : StoryPage.BridgeCage));
                Assert.That(run.CagesExplored, Is.EqualTo(fall ? cageNumber - 1 : cageNumber));
                run.Validate();
            }
        }

        [TestCase(CombatantId.Warrior, CageContent.MirrorShield)]
        [TestCase(CombatantId.Mage, CageContent.LetterAndAmulet)]
        public void BridgeItemsAreCollectedOnceAndRestrictedByClass(CombatantId hero, CageContent equipment)
        {
            var run = BridgeReady(hero);
            run.EnterCage(Array.IndexOf(run.cages, (int)equipment), new ScriptedRandomSource());
            Assert.That(run.CollectBridgeItem(), Is.True);
            Assert.That(run.CollectBridgeItem(), Is.False);
            run.LeaveCage();
            var other = hero == CombatantId.Warrior ? CageContent.LetterAndAmulet : CageContent.MirrorShield;
            run.EnterCage(Array.IndexOf(run.cages, (int)other), new ScriptedRandomSource(99));
            Assert.That(run.CollectBridgeItem(), Is.False); run.Validate();
        }

        [Test]
        public void RevisitingACageDoesNotRollOrCountAgainAndBridgeCanBeSkipped()
        {
            var run = BridgeReady();
            run.EnterCage(0, new ScriptedRandomSource()); run.LeaveCage();
            run.EnterCage(0, new ScriptedRandomSource());
            Assert.That(run.CagesExplored, Is.EqualTo(1)); run.LeaveCage(); run.LeaveBridge(); run.Validate();
            var skipped = BridgeReady(); skipped.LeaveBridge();
            Assert.That(skipped.page, Is.EqualTo(StoryPage.Courtyard));
            Assert.That(skipped.CagesExplored, Is.Zero); skipped.Validate();
        }

        [Test]
        public void BridgeKeysAndFlintAreCollectedOnce()
        {
            var run = BridgeReady();
            foreach (var item in new[] { CageContent.RedKey, CageContent.Flint, CageContent.BlueKey })
            {
                run.EnterCage(Array.IndexOf(run.cages, (int)item), new ScriptedRandomSource(99));
                Assert.That(run.CollectBridgeItem(), Is.True);
                Assert.That(run.CollectBridgeItem(), Is.False);
                run.LeaveCage(); run.Validate();
            }
            Assert.That(run.redKey && run.flint && run.blueKey, Is.True);
        }

        [Test]
        public void BridgeCheckpointRejectsInvalidLayoutsAndItems()
        {
            var run = BridgeReady(); run.cages[0] = run.cages[1];
            Assert.Throws<ArgumentException>(() => run.Validate());
            run = BridgeReady(); run.redKey = true;
            Assert.Throws<ArgumentException>(() => run.Validate());
            run = BridgeReady(CombatantId.Mage); run.mirrorShield = true;
            run.cagesVisited = 1 << (int)CageContent.MirrorShield;
            Assert.Throws<ArgumentException>(() => run.Validate());
        }

        [TestCase(CombatantId.Warrior, CageContent.MirrorShield)]
        [TestCase(CombatantId.Mage, CageContent.LetterAndAmulet)]
        public void PortalRestoresGoblinProgressAndKeepsTheCageLayout(CombatantId hero, CageContent equipment)
        {
            var run = BridgeReady(hero); var layout = (int[])run.cages.Clone();
            var defense = hero == CombatantId.Warrior ? 24 : 6;
            run.equipment = true; run.goblinExperience = true; run.rewardAttack = 32; run.rewardDefense = defense;
            run.visited = 1; run.crystal = true; run.food = run.foodCollected = true;
            run.EnterCage(Array.IndexOf(run.cages, (int)equipment), new ScriptedRandomSource());
            Assert.That(run.CollectBridgeItem(), Is.True); run.LeaveCage();
            run.EnterCage(Array.IndexOf(run.cages, (int)CageContent.RedKey), new ScriptedRandomSource(99));
            Assert.That(run.CollectBridgeItem(), Is.True); run.LeaveCage(); run.LeaveBridge();
            Assert.Throws<InvalidOperationException>(() => run.ReturnToColdForest());
            run.page = StoryPage.Anteroom;
            Assert.Throws<ArgumentException>(() => run.ReturnToColdForest());
            run.knightDefeated = true; run.dialogue = 5; run.castleSeen = true; run.skeletonExperience = true;

            run.ReturnToColdForest();

            Assert.That(run.page, Is.EqualTo(StoryPage.ColdForest)); Assert.That(run.dialogue, Is.Zero);
            Assert.That(run.reward, Is.EqualTo(ItemId.HealingPotion)); Assert.That(run.goblinExperience, Is.True);
            Assert.That(run.victoryHealth, Is.EqualTo(70)); Assert.That(run.rewardAttack, Is.EqualTo(32));
            Assert.That(run.rewardDefense, Is.EqualTo(defense)); Assert.That(run.equipment, Is.True);
            Assert.That(run.crystal && run.food && run.foodCollected, Is.True);
            Assert.That(run.graves, Is.Null); Assert.That(run.skeletonAppeared, Is.False);
            Assert.That(run.skeletonDefeated, Is.False); Assert.That(run.skeletonReward, Is.EqualTo(ItemId.None));
            Assert.That(run.skeletonExperience, Is.False); Assert.That(run.castleSeen, Is.False);
            Assert.That(run.knightDefeated, Is.False);
            Assert.That(run.cages, Is.EqualTo(layout)); Assert.That(run.CagesExplored, Is.Zero);
            Assert.That(run.redKey || run.blueKey || run.flint || run.mirrorShield || run.amethystAmulet, Is.False);
            run.Validate();

            run.PrepareGraveyard(); run.page = StoryPage.Graveyard;
            run.EnterGrave(run.skeletonGraveNumber - 1);
            var player = hero == CombatantId.Warrior ? TestContent.Warrior() : TestContent.Mage();
            var pool = new[] { TestContent.Item(ItemId.HealingPotion, 3), TestContent.Item(ItemId.AttackGem, 3) };
            var goblin = new EncounterConfiguration(TestContent.Goblin(), 15, pool);
            var skeleton = new EncounterConfiguration(TestContent.Enemy(CombatantId.Skeleton, 62, 20, 5), 15, pool);
            var battle = StoryEncounter.CreateSkeleton(run, player, goblin, skeleton);
            Assert.That(battle.State.Hero.CurrentHealth, Is.EqualTo(70));
            Assert.That(battle.State.Hero.Stats.Attack, Is.EqualTo(32));
            Assert.That(battle.State.Hero.Stats.Defense, Is.EqualTo(defense));
        }

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
            Assert.That(triggers.OrderBy(x => x), Is.EqualTo(new[] { 4, 5, 6 }));
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
            run.page = StoryPage.ColdForest; run.PrepareGraveyard(); run.skeletonTrigger = 4;
            run.reward = ItemId.HealingPotion; run.victoryHealth = 70; run.rewardAttack = 30;
            var original = (int[])run.graves.Clone();
            var letter = Array.IndexOf(run.graves, (int)GraveContent.LucyLetter);
            var first = Enumerable.Range(0, 6).Where(i => i != letter).Take(3).ToArray();
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

        [TestCase(CombatantId.Warrior, false)]
        [TestCase(CombatantId.Mage, false)]
        [TestCase(CombatantId.Warrior, true)]
        [TestCase(CombatantId.Mage, true)]
        public void SkeletonRetryPreservesGoblinRewardHealthAndEquipment(CombatantId id, bool experience)
        {
            var hero = id == CombatantId.Warrior ? TestContent.Warrior() : TestContent.Mage();
            var pool = new[] { TestContent.Item(ItemId.AttackGem, 3, new ItemModifier(ItemModifierKind.Attack, 10)), TestContent.Item(ItemId.HealingPotion, 3) };
            var goblin = new EncounterConfiguration(TestContent.Goblin(), 15, pool);
            var skeleton = new EncounterConfiguration(TestContent.Enemy(CombatantId.Skeleton, 62, 20, 5), 15, pool);
            var run = StoryRun.Create(id, 417); run.page = StoryPage.ColdForest; run.PrepareGraveyard();
            run.page = StoryPage.SkeletonEmergence; run.skeletonAppeared = true; run.equipment = true;
            run.gravesVisited = (1 << (run.skeletonTrigger - 1)) - 1; run.activeGrave = run.skeletonTrigger - 1;
            run.reward = ItemId.AttackGem; run.goblinExperience = experience; run.victoryHealth = 70;
            run.rewardAttack = 40 + (experience ? 2 : 0); run.rewardDefense = hero.BaseStats.Defense + (experience ? 1 : 0);
            var first = StoryEncounter.CreateSkeleton(run, hero, goblin, skeleton);
            var retry = StoryEncounter.CreateSkeleton(run, hero, goblin, skeleton);
            Assert.That(first.State.Hero.CurrentHealth, Is.EqualTo(70)); Assert.That(first.State.Hero.Stats.Attack, Is.EqualTo(run.rewardAttack));
            Assert.That(first.State.Hero.Stats.Defense, Is.EqualTo(run.rewardDefense));
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
            Assert.That(run.skeletonExperience, Is.True);
            Assert.That(run.skeletonDefense, Is.EqualTo(run.rewardDefense + 1));
            Assert.Throws<InvalidOperationException>(() => StoryEncounter.ChooseSkeletonReward(run, skeleton, offer[1].Id, restored));
        }

        [TestCase(CombatantId.Warrior, false, false)]
        [TestCase(CombatantId.Warrior, false, true)]
        [TestCase(CombatantId.Warrior, true, false)]
        [TestCase(CombatantId.Warrior, true, true)]
        [TestCase(CombatantId.Mage, false, false)]
        [TestCase(CombatantId.Mage, false, true)]
        [TestCase(CombatantId.Mage, true, false)]
        [TestCase(CombatantId.Mage, true, true)]
        public void SkeletonLoadsKnownAttributeVersionsAndRejectsAlteredRewards(CombatantId id, bool legacy, bool experience)
        {
            var current = id == CombatantId.Warrior ? new CombatantStats(150, 25, 20) : new CombatantStats(120, 30, 5);
            var previous = id == CombatantId.Warrior ? TestContent.Warrior().BaseStats : TestContent.Mage().BaseStats;
            var saved = legacy ? previous : current;
            var hero = TestContent.Hero(id, current, AbilityId.BasicAttack);
            var pool = new[] { TestContent.Item(ItemId.AttackGem, 3, new ItemModifier(ItemModifierKind.Attack, 10)), TestContent.Item(ItemId.HealingPotion, 3) };
            var goblin = new EncounterConfiguration(TestContent.Goblin(), 15, pool);
            var run = StoryRun.Create(id, 417); run.page = StoryPage.ColdForest; run.PrepareGraveyard();
            run.skeletonAppeared = true; run.page = StoryPage.SkeletonEmergence;
            run.gravesVisited = (1 << (run.skeletonTrigger - 1)) - 1; run.activeGrave = run.skeletonTrigger - 1;
            run.reward = ItemId.AttackGem; run.goblinExperience = experience; run.victoryHealth = 70;
            run.rewardAttack = saved.Attack + 10 + (experience ? 2 : 0);
            run.rewardDefense = saved.Defense + (experience ? 1 : 0);

            var restored = StoryEncounter.RestoreSkeletonHero(run, hero, goblin, false);
            Assert.That(restored.CurrentHealth, Is.EqualTo(70));
            Assert.That(restored.Stats.Attack, Is.EqualTo(run.rewardAttack));
            Assert.That(restored.Stats.Defense, Is.EqualTo(run.rewardDefense));
            run.rewardAttack++;
            Assert.Throws<ArgumentException>(() => StoryEncounter.RestoreSkeletonHero(run, hero, goblin, false));
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
