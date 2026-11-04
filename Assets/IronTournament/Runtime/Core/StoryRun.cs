using System;
using System.Collections.Generic;

namespace IronTournament.Core
{
    public enum StoryPage { Intro, Village, House, Rescue, Battle, AfterBattle, Reward, Farewell, Finished,
        Title, Chapter, Arrival, Approach, Food, LootLead, Burial, Exit }
    public enum ChildFate { Saved, Dying, Dead, Absent }

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

        public int ExploredCount
        {
            get { var n = 0; for (var i = 0; i < 5; i++) if ((visited & (1 << i)) != 0) n++; return n; }
        }
        public ChildFate Fate => secondCampaign ? ChildFate.Absent : lastCallDeclined ? ChildFate.Dead
            : ExploredCount >= 3 ? ChildFate.Dying : ChildFate.Saved;

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
                rewardAttack < 0 || rewardAttack > 9999 || rewardDefense < 0 || rewardDefense > 9999 || !Enum.IsDefined(typeof(ItemId), reward))
                throw new ArgumentException("Invalid story checkpoint.");
            var mask = 0;
            foreach (var h in houses) { if (h < 0 || h > 4 || (mask & (1 << h)) != 0) throw new ArgumentException("Invalid house layout."); mask |= 1 << h; }
            if ((page == StoryPage.House && activeHouse < 0) ||
                (food && (!foodCollected || foodGiven)) || (foodGiven && (!foodCollected || Fate != ChildFate.Saved)) ||
                (lastCallDeclined && ExploredCount < 3) || (crystal && (secondCampaign || Fate != ChildFate.Saved)) ||
                (pendingCry && (page != StoryPage.Village || secondCampaign || ExploredCount < 1 || ExploredCount > 3)))
                throw new ArgumentException("Inconsistent story checkpoint.");
        }
    }

}
