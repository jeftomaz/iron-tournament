using System;
using System.Collections.Generic;

namespace IronTournament.Core
{
    public enum StoryPage { Intro, Village, House, Rescue, Battle, AfterBattle, Reward, Farewell, Finished,
        Title, Chapter, Arrival, Approach, Food, LootLead, Burial, Exit,
        ColdForest, Graveyard, Grave, SkeletonEmergence, SkeletonBattle, SkeletonLetter, SkeletonReward,
        Castle, SkeletonFinished }
    public enum ChildFate { Saved, Dying, Dead, Absent }
    public enum GraveContent { LucyLetter, SamuelLetter, Rosary, Doll, Cat, Empty }

    [Serializable]
    public sealed class StoryRun
    {
        public int version = 1;
        public int seed;
        public CombatantId hero;
        public StoryPage page;
        public int[] houses;
        public int visited;
        public int activeHouse = -1;
        public bool room;
        public int dialogue;
        public bool lastCallDeclined;
        public bool food;
        public bool foodCollected;
        public bool foodGiven;
        public bool equipment;
        public bool crystal;
        public bool secondCampaign;
        public bool previousChildSaved;
        public int completionOrigin;
        public ItemId reward;
        public bool pendingCry;
        public int victoryHealth;
        public int rewardAttack;
        public int rewardDefense;
        public int[] graves;
        public int gravesVisited;
        public int activeGrave = -1;
        public int skeletonTrigger;
        public bool skeletonAppeared;
        public bool skeletonDefeated;
        public int skeletonHealth;
        public int skeletonScrolls;
        public int skeletonHorns;
        public bool skeletonGuardianArmed;
        public ItemId skeletonReward;
        public int skeletonAttack;
        public int skeletonDefense;
        public bool castleSeen;

        public int ExploredCount
        {
            get { var n = 0; for (var i = 0; i < 5; i++) if ((visited & (1 << i)) != 0) n++; return n; }
        }
        public ChildFate Fate => secondCampaign ? ChildFate.Absent : lastCallDeclined ? ChildFate.Dead
            : ExploredCount >= 3 ? ChildFate.Dying : ChildFate.Saved;

        public int GravesExplored
        {
            get { var n = 0; for (var i = 0; i < 6; i++) if ((gravesVisited & (1 << i)) != 0) n++; return n; }
        }

        public void PrepareGraveyard()
        {
            if (page != StoryPage.ColdForest) throw new InvalidOperationException("The graveyard is not available.");
            if (graves != null && graves.Length != 0) return;
            var random = new SeededRandomSource(seed ^ 0x37b9a6d1);
            var pairs = new[] { 0, 1, 3, 4, 6, 7, 8 };
            var pair = pairs[random.Next(0, pairs.Length)];
            var first = pair < 6 ? pair : pair - 6;
            var second = pair < 6 ? pair + 1 : first + 3;
            graves = new[] { -1, -1, -1, -1, -1, -1 };
            if (random.Next(0, 2) == 0) { graves[first] = 0; graves[second] = 1; }
            else { graves[first] = 1; graves[second] = 0; }
            var remaining = new[] { 2, 3, 4, 5 };
            for (var i = remaining.Length - 1; i > 0; i--)
            { var j = random.Next(0, i + 1); var value = remaining[i]; remaining[i] = remaining[j]; remaining[j] = value; }
            var index = 0;
            for (var i = 0; i < graves.Length; i++) if (graves[i] < 0) graves[i] = remaining[index++];
            skeletonTrigger = random.Next(3, 7);
        }

        public void EnterGrave(int slot)
        {
            if (page != StoryPage.Graveyard || graves == null || slot < 0 || slot >= graves.Length)
                throw new InvalidOperationException("Invalid grave entry.");
            activeGrave = slot;
            if (!skeletonAppeared && (gravesVisited & (1 << slot)) == 0 && GravesExplored + 1 == skeletonTrigger)
            { skeletonAppeared = true; page = StoryPage.SkeletonEmergence; return; }
            gravesVisited |= 1 << slot;
            page = StoryPage.Grave;
        }

        public static StoryRun Create(CombatantId hero, int seed, CombatantId completedHero = CombatantId.None,
            bool childSaved = false, int completion = 0)
        {
            if (hero != CombatantId.Warrior && hero != CombatantId.Mage) throw new ArgumentOutOfRangeException(nameof(hero));
            var run = new StoryRun { hero = hero, seed = seed, houses = new[] { 0, 1, 2, 3, 4 },
                secondCampaign = completedHero != CombatantId.None && completedHero != hero,
                previousChildSaved = childSaved, completionOrigin = completion };
            var random = new SeededRandomSource(seed);
            for (var i = 4; i > 0; i--)
            {
                var j = random.Next(0, i + 1);
                var value = run.houses[i]; run.houses[i] = run.houses[j]; run.houses[j] = value;
            }
            return run;
        }

        public void EnterHouse(int slot)
        {
            if (page != StoryPage.Village || slot < 0 || slot >= 5) throw new InvalidOperationException("Invalid house entry.");
            activeHouse = houses[slot]; room = false; page = StoryPage.House;
        }

        public bool LeaveHouse()
        {
            if (page != StoryPage.House) throw new InvalidOperationException("No house is open.");
            var fresh = (visited & (1 << activeHouse)) == 0;
            visited |= 1 << activeHouse;
            room = false; page = StoryPage.Village;
            pendingCry = fresh && !secondCampaign && ExploredCount <= 3;
            return pendingCry;
        }

        public bool CollectEquipment()
        {
            if (page != StoryPage.House || equipment ||
                activeHouse != (hero == CombatantId.Warrior ? 0 : 1)) return false;
            equipment = true; return true;
        }

        public bool CollectFood()
        {
            if (page != StoryPage.House || activeHouse != 3 || foodCollected) return false;
            food = foodCollected = true; return true;
        }

        public bool GiveFood()
        {
            if (page != StoryPage.AfterBattle || Fate != ChildFate.Saved || !food) return false;
            food = false; foodGiven = true; return true;
        }

        public void BeginRescue()
        {
            if (page != StoryPage.Village || (!secondCampaign && ExploredCount == 0))
                throw new InvalidOperationException("The forest is not available.");
            pendingCry = false; page = StoryPage.Approach; dialogue = 0;
        }

        public void ContinueExploration()
        {
            if (page != StoryPage.Village) throw new InvalidOperationException("Not in the village.");
            if (pendingCry && ExploredCount == 3) lastCallDeclined = true;
            pendingCry = false;
        }

        public void Validate()
        {
            if (version != 1 || (hero != CombatantId.Warrior && hero != CombatantId.Mage) ||
                !Enum.IsDefined(typeof(StoryPage), page) || houses == null || houses.Length != 5 ||
                visited < 0 || visited > 31 || activeHouse < -1 || activeHouse > 4 || dialogue < 0 || dialogue > 80 ||
                completionOrigin < 0 || victoryHealth < 0 || victoryHealth > 9999 ||
                rewardAttack < 0 || rewardAttack > 9999 || rewardDefense < 0 || rewardDefense > 9999 || !Enum.IsDefined(typeof(ItemId), reward) ||
                gravesVisited < 0 || gravesVisited > 63 || activeGrave < -1 || activeGrave > 5 || skeletonHealth < 0 || skeletonHealth > 9999 ||
                skeletonScrolls < 0 || skeletonScrolls > 2 || skeletonHorns < 0 || skeletonHorns > 2 || !Enum.IsDefined(typeof(ItemId), skeletonReward) ||
                skeletonAttack < 0 || skeletonAttack > 9999 || skeletonDefense < 0 || skeletonDefense > 9999)
                throw new ArgumentException("Invalid story checkpoint.");
            var mask = 0;
            foreach (var h in houses) { if (h < 0 || h > 4 || (mask & (1 << h)) != 0) throw new ArgumentException("Invalid house layout."); mask |= 1 << h; }
            var hasGraves = graves != null && graves.Length != 0;
            if (hasGraves)
            {
                if (graves.Length != 6 || skeletonTrigger < 3 || skeletonTrigger > 6) throw new ArgumentException("Invalid grave layout.");
                mask = 0; var lucy = -1; var samuel = -1;
                for (var i = 0; i < 6; i++)
                {
                    var value = graves[i];
                    if (value < 0 || value > 5 || (mask & (1 << value)) != 0) throw new ArgumentException("Invalid grave layout.");
                    mask |= 1 << value;
                    if (value == 0) lucy = i; else if (value == 1) samuel = i;
                }
                if (!(Math.Abs(lucy - samuel) == 3 || (lucy / 3 == samuel / 3 && Math.Abs(lucy - samuel) == 1)))
                    throw new ArgumentException("Letters must be adjacent.");
            }
            if ((page == StoryPage.House && activeHouse < 0) ||
                (room && (page != StoryPage.House || activeHouse < 2)) ||
                (food && (!foodCollected || foodGiven)) || (foodGiven && (!foodCollected || Fate != ChildFate.Saved)) ||
                (lastCallDeclined && ExploredCount < 3) || (crystal && (secondCampaign || Fate != ChildFate.Saved)) ||
                (pendingCry && (page != StoryPage.Village || secondCampaign || ExploredCount < 1 || ExploredCount > 3)) ||
                (!hasGraves && (gravesVisited != 0 || skeletonTrigger != 0 || skeletonAppeared || skeletonDefeated)) ||
                ((int)page >= (int)StoryPage.Graveyard && !hasGraves) ||
                ((int)page >= (int)StoryPage.ColdForest && (reward == ItemId.None || victoryHealth <= 0 || rewardAttack <= 0)) ||
                (page == StoryPage.Grave && (!hasGraves || activeGrave < 0 || (gravesVisited & (1 << activeGrave)) == 0)) ||
                (skeletonAppeared && (activeGrave < 0 || GravesExplored < skeletonTrigger - 1)) ||
                (hasGraves && !skeletonAppeared && GravesExplored >= skeletonTrigger) ||
                ((page == StoryPage.SkeletonEmergence || page == StoryPage.SkeletonBattle) && (!skeletonAppeared || skeletonDefeated)) ||
                ((page == StoryPage.SkeletonLetter || page == StoryPage.SkeletonReward || page == StoryPage.Castle || page == StoryPage.SkeletonFinished) && !skeletonDefeated) ||
                (skeletonDefeated && (!skeletonAppeared || skeletonHealth <= 0)) ||
                (!skeletonDefeated && (skeletonHealth != 0 || skeletonScrolls != 0 || skeletonHorns != 0 || skeletonGuardianArmed)) ||
                (page == StoryPage.SkeletonReward && skeletonReward != ItemId.None) ||
                (skeletonReward != ItemId.None && !skeletonDefeated) || (castleSeen && skeletonReward == ItemId.None) ||
                ((page == StoryPage.Castle || page == StoryPage.SkeletonFinished) && skeletonReward == ItemId.None))
                throw new ArgumentException("Inconsistent story checkpoint.");
        }
    }

}
