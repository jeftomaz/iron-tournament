using System;
using System.Collections.Generic;

namespace IronTournament.Core
{
    public enum StoryPage { Intro, Village, House, Rescue, Battle, AfterBattle, Reward, Farewell, Finished,
        Title, Chapter, Arrival, Approach, Food, LootLead, Burial, Exit,
        ColdForest, Graveyard, Grave, SkeletonEmergence, SkeletonBattle, SkeletonLetter, SkeletonReward,
        Castle, SkeletonFinished, Bridge, BridgeCage, BridgeFall, Courtyard, Anteroom,
        KnightBattle, KnightReward, CarnalHall, Cauldron, WerewolfBattle, WerewolfReward, WerewolfEmergence }
    public enum ChildFate { Saved, Dying, Dead, Absent }
    public enum GraveContent { LucyLetter, SamuelLetter, Rosary, Doll, Cat, Empty, Skeleton }
    public enum CageContent { Remains, LetterAndAmulet, RedKey, MirrorShield, Flint, BlueKey }

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
        public bool goblinExperience;
        public int[] graves;
        public int gravesVisited;
        public int activeGrave = -1;
        public int skeletonTrigger;
        public int skeletonGraveNumber;
        public bool skeletonAppeared;
        public bool skeletonDefeated;
        public int skeletonHealth;
        public int skeletonScrolls;
        public int skeletonHorns;
        public bool skeletonGuardianArmed;
        public ItemId skeletonReward;
        public int skeletonAttack;
        public int skeletonDefense;
        public bool skeletonExperience;
        public bool castleSeen;
        public int[] cages;
        public int cagesVisited;
        public int activeCage = -1;
        public bool bridgeFell;
        public bool redKey;
        public bool blueKey;
        public bool flint;
        public bool mirrorShield;
        public bool amethystAmulet;
        public bool knightDefeated;
        public int knightHealth;
        public int knightScrolls;
        public int knightHorns;
        public bool knightGuardianArmed;
        public ItemId knightReward;
        public int knightAttack;
        public int knightDefense;
        public int werewolfHealth;
        public int werewolfScrolls;
        public int werewolfHorns;
        public bool werewolfGuardianArmed;
        public ItemId werewolfReward;
        public int werewolfAttack;
        public int werewolfDefense;
        public int cauldronsVisited;
        public int activeCauldron = -1;
        public bool werewolfAppeared;
        public bool werewolfDefeated;
        public bool purpleKey;
        public bool woodenStake;
        public bool werewolfBurned;
        public bool anteroomTalked;
        public bool courtyardTalked;
        public bool hallIntroSeen;

        public int CauldronsExplored
        {
            get { var n = 0; for (var i = 0; i < 5; i++) if ((cauldronsVisited & (1 << i)) != 0) n++; return n; }
        }

        public void EnterCarnalHall()
        {
            if (page != StoryPage.Anteroom || !redKey || purpleKey || knightReward == ItemId.None)
                throw new InvalidOperationException("The carnal hall is not available.");
            Validate();
            page = StoryPage.CarnalHall; dialogue = 0;
        }

        public void EnterCauldron(int slot, IRandomSource random)
        {
            if (page != StoryPage.CarnalHall || slot < 0 || slot >= 5)
                throw new InvalidOperationException("Invalid cauldron entry.");
            if (random == null) throw new ArgumentNullException(nameof(random));
            Validate();
            var fresh = (cauldronsVisited & (1 << slot)) == 0;
            var count = CauldronsExplored;
            var chances = new[] { 0, 10, 20, 30, 60 };
            if (!werewolfAppeared && fresh && count > 0 && random.Next(0, 100) < chances[count])
            { werewolfAppeared = true; page = StoryPage.WerewolfEmergence; dialogue = 0; return; }
            cauldronsVisited |= 1 << slot; activeCauldron = slot; page = StoryPage.Cauldron;
        }

        public bool CollectCauldronItem()
        {
            if (page != StoryPage.Cauldron) return false;
            Validate();
            if (activeCauldron == 1 && !woodenStake) { woodenStake = true; return true; }
            if (activeCauldron == 4 && !purpleKey) { purpleKey = true; redKey = false; return true; }
            return false;
        }

        public void LeaveCauldron()
        {
            if (page != StoryPage.Cauldron) throw new InvalidOperationException("No cauldron is open.");
            Validate(); activeCauldron = -1;
            if (!werewolfAppeared && CauldronsExplored == 5)
            { werewolfAppeared = true; page = StoryPage.WerewolfEmergence; dialogue = 0; }
            else page = StoryPage.CarnalHall;
        }

        public void LeaveCarnalHall()
        {
            if (page != StoryPage.CarnalHall || !werewolfDefeated || werewolfReward == ItemId.None)
                throw new InvalidOperationException("The hall is still locked.");
            Validate(); page = StoryPage.Anteroom; dialogue = 0; anteroomTalked = false;
        }

        public bool BurnWerewolf()
        {
            if (page != StoryPage.CarnalHall || !werewolfDefeated || werewolfReward == ItemId.None || !flint || werewolfBurned) return false;
            Validate(); werewolfBurned = true; return true;
        }

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

        public int CagesExplored
        {
            get { var n = 0; for (var i = 0; i < 6; i++) if ((cagesVisited & (1 << i)) != 0) n++; return n; }
        }

        public void EnterBridge()
        {
            if (page != StoryPage.SkeletonFinished || skeletonReward == ItemId.None)
                throw new InvalidOperationException("The bridge is not available.");
            Validate();
            if (cages == null || cages.Length == 0)
            {
                cages = new[] { 0, 1, 2, 3, 4, 5 };
                var random = new SeededRandomSource(seed ^ 0x1a22b76d);
                for (var i = cages.Length - 1; i > 0; i--)
                { var j = random.Next(0, i + 1); var value = cages[i]; cages[i] = cages[j]; cages[j] = value; }
            }
            activeCage = -1; page = StoryPage.Bridge;
        }

        public void EnterCage(int slot, IRandomSource random)
        {
            if (page != StoryPage.Bridge || cages == null || cages.Length != 6 || slot < 0 || slot >= 6)
                throw new InvalidOperationException("Invalid cage entry.");
            if (random == null) throw new ArgumentNullException(nameof(random));
            Validate();
            var cage = cages[slot];
            var fresh = (cagesVisited & (1 << cage)) == 0;
            var count = CagesExplored;
            var chances = new[] { 0, 5, 15, 25, 35, 50 };
            if (fresh && count > 0 && random.Next(0, 100) < chances[count])
            { bridgeFell = true; activeCage = -1; page = StoryPage.BridgeFall; return; }
            cagesVisited |= 1 << cage; activeCage = cage; page = StoryPage.BridgeCage;
        }

        public bool CollectBridgeItem()
        {
            if (page != StoryPage.BridgeCage) return false;
            Validate();
            switch ((CageContent)activeCage)
            {
                case CageContent.LetterAndAmulet:
                    if (hero != CombatantId.Mage || amethystAmulet) return false;
                    amethystAmulet = true; return true;
                case CageContent.RedKey:
                    if (redKey) return false;
                    redKey = true; return true;
                case CageContent.MirrorShield:
                    if (hero != CombatantId.Warrior || mirrorShield) return false;
                    mirrorShield = true; return true;
                case CageContent.Flint:
                    if (flint) return false;
                    flint = true; return true;
                case CageContent.BlueKey:
                    if (blueKey) return false;
                    blueKey = true; return true;
                default: return false;
            }
        }

        public void LeaveCage()
        {
            if (page != StoryPage.BridgeCage) throw new InvalidOperationException("No cage is open.");
            Validate();
            activeCage = -1; page = StoryPage.Bridge;
        }

        public void LeaveBridge()
        {
            if (page != StoryPage.Bridge) throw new InvalidOperationException("The courtyard is not available.");
            Validate();
            page = StoryPage.Courtyard;
        }

        public void ReturnToColdForest()
        {
            if (page != StoryPage.Anteroom) throw new InvalidOperationException("The portal is not available.");
            Validate();
            graves = null; gravesVisited = 0; activeGrave = -1; skeletonTrigger = 0; skeletonGraveNumber = 0;
            skeletonAppeared = false; skeletonDefeated = false; skeletonHealth = 0; skeletonScrolls = 0;
            skeletonHorns = 0; skeletonGuardianArmed = false; skeletonReward = ItemId.None;
            skeletonAttack = 0; skeletonDefense = 0; skeletonExperience = false; castleSeen = false;
            cagesVisited = 0; activeCage = -1; bridgeFell = false; redKey = false; blueKey = false;
            flint = false; mirrorShield = false; amethystAmulet = false; knightDefeated = false;
            knightHealth = 0; knightScrolls = 0; knightHorns = 0; knightGuardianArmed = false;
            knightReward = ItemId.None; knightAttack = 0; knightDefense = 0;
            werewolfHealth = 0; werewolfScrolls = 0; werewolfHorns = 0; werewolfGuardianArmed = false;
            werewolfReward = ItemId.None; werewolfAttack = 0; werewolfDefense = 0;
            cauldronsVisited = 0; activeCauldron = -1; werewolfAppeared = false; werewolfDefeated = false;
            purpleKey = false; woodenStake = false; werewolfBurned = false;
            anteroomTalked = false;
            courtyardTalked = false;
            hallIntroSeen = false;
            page = StoryPage.ColdForest; dialogue = 0;
            Validate();
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
            skeletonTrigger = random.Next(4, 7);
            if (crystal) skeletonGraveNumber = Array.IndexOf(graves, (int)GraveContent.Empty) + 1;
        }

        public void MigrateLegacyGrave()
        {
            if (graves == null || graves.Length == 0 || skeletonGraveNumber != 0) return;
            if (skeletonAppeared)
            {
                if (!skeletonDefeated || page == StoryPage.SkeletonLetter || page == StoryPage.SkeletonReward || page == StoryPage.Castle)
                { skeletonGraveNumber = activeGrave + 1; graves[activeGrave] = (int)GraveContent.Skeleton; }
                return;
            }
            if (!crystal) return;
            var empty = Array.IndexOf(graves, (int)GraveContent.Empty);
            if (empty < 0) throw new ArgumentException("No grave available for the skeleton.");
            skeletonGraveNumber = empty + 1;
        }

        public bool TriggersSkeleton(int slot)
        {
            if (slot < 0 || slot >= 6 || skeletonAppeared) return false;
            if (crystal && skeletonGraveNumber != 0) return slot + 1 == skeletonGraveNumber;
            return (gravesVisited & (1 << slot)) == 0 && GravesExplored + 1 == skeletonTrigger;
        }

        public void EnterGrave(int slot)
        {
            if (page != StoryPage.Graveyard || graves == null || slot < 0 || slot >= graves.Length)
                throw new InvalidOperationException("Invalid grave entry.");
            activeGrave = slot;
            if (TriggersSkeleton(slot))
            {
                skeletonAppeared = true; skeletonGraveNumber = slot + 1;
                graves[slot] = (int)GraveContent.Skeleton; page = StoryPage.SkeletonEmergence; return;
            }
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
                gravesVisited < 0 || gravesVisited > 63 || activeGrave < -1 || activeGrave > 5 || skeletonGraveNumber < 0 || skeletonGraveNumber > 6 || skeletonHealth < 0 || skeletonHealth > 9999 ||
                skeletonScrolls < 0 || skeletonScrolls > 2 || skeletonHorns < 0 || skeletonHorns > 2 || !Enum.IsDefined(typeof(ItemId), skeletonReward) ||
                skeletonAttack < 0 || skeletonAttack > 9999 || skeletonDefense < 0 || skeletonDefense > 9999 ||
                cagesVisited < 0 || cagesVisited > 63 || activeCage < -1 || activeCage > 5 ||
                cauldronsVisited < 0 || cauldronsVisited > 31 || activeCauldron < -1 || activeCauldron > 4 ||
                knightHealth < 0 || knightHealth > 9999 || knightScrolls < 0 || knightScrolls > 3 || knightHorns < 0 || knightHorns > 3 ||
                knightAttack < 0 || knightAttack > 9999 || knightDefense < 0 || knightDefense > 9999 || !Enum.IsDefined(typeof(ItemId), knightReward) ||
                werewolfHealth < 0 || werewolfHealth > 9999 || werewolfScrolls < 0 || werewolfScrolls > 4 || werewolfHorns < 0 || werewolfHorns > 4 ||
                werewolfAttack < 0 || werewolfAttack > 9999 || werewolfDefense < 0 || werewolfDefense > 9999 || !Enum.IsDefined(typeof(ItemId), werewolfReward))
                throw new ArgumentException("Invalid story checkpoint.");
            var mask = 0;
            foreach (var h in houses) { if (h < 0 || h > 4 || (mask & (1 << h)) != 0) throw new ArgumentException("Invalid house layout."); mask |= 1 << h; }
            var hasGraves = graves != null && graves.Length != 0;
            var hasCages = cages != null && cages.Length != 0;
            if (hasCages)
            {
                if (cages.Length != 6) throw new ArgumentException("Invalid cage layout.");
                mask = 0;
                foreach (var cage in cages)
                { if (cage < 0 || cage > 5 || (mask & (1 << cage)) != 0) throw new ArgumentException("Invalid cage layout."); mask |= 1 << cage; }
            }
            if (hasGraves)
            {
                if (graves.Length != 6 || skeletonTrigger < 3 || skeletonTrigger > 6) throw new ArgumentException("Invalid grave layout.");
                mask = 0; var lucy = -1; var samuel = -1;
                for (var i = 0; i < 6; i++)
                {
                    var value = graves[i];
                    var maximum = skeletonAppeared && skeletonGraveNumber != 0 ? (int)GraveContent.Skeleton : (int)GraveContent.Empty;
                    if (value < 0 || value > maximum || (mask & (1 << value)) != 0) throw new ArgumentException("Invalid grave layout.");
                    mask |= 1 << value;
                    if (value == 0) lucy = i; else if (value == 1) samuel = i;
                }
                if (lucy >= 0 && samuel >= 0 && !(Math.Abs(lucy - samuel) == 3 || (lucy / 3 == samuel / 3 && Math.Abs(lucy - samuel) == 1)))
                    throw new ArgumentException("Letters must be adjacent.");
                if ((lucy < 0 || samuel < 0) && (!skeletonAppeared || skeletonGraveNumber == 0))
                    throw new ArgumentException("Missing grave letter.");
                if (skeletonGraveNumber != 0 && graves[skeletonGraveNumber - 1] !=
                    (int)(skeletonAppeared ? GraveContent.Skeleton : GraveContent.Empty))
                    throw new ArgumentException("Invalid skeleton grave.");
            }
            if ((page == StoryPage.House && activeHouse < 0) ||
                (room && (page != StoryPage.House || activeHouse < 2)) ||
                (food && (!foodCollected || foodGiven)) || (foodGiven && (!foodCollected || Fate != ChildFate.Saved)) ||
                (lastCallDeclined && ExploredCount < 3) || (crystal && (secondCampaign || Fate != ChildFate.Saved)) ||
                (pendingCry && (page != StoryPage.Village || secondCampaign || ExploredCount < 1 || ExploredCount > 3)) ||
                (!hasGraves && (gravesVisited != 0 || skeletonTrigger != 0 || skeletonGraveNumber != 0 || skeletonAppeared || skeletonDefeated)) ||
                (skeletonGraveNumber != 0 && !crystal && !skeletonAppeared) ||
                ((int)page >= (int)StoryPage.Graveyard && !hasGraves) ||
                ((int)page >= (int)StoryPage.ColdForest && (reward == ItemId.None || victoryHealth <= 0 || rewardAttack <= 0)) ||
                (page == StoryPage.Grave && (!hasGraves || activeGrave < 0 || (gravesVisited & (1 << activeGrave)) == 0)) ||
                (skeletonAppeared && (activeGrave < 0 || (!crystal && GravesExplored < skeletonTrigger - 1))) ||
                (hasGraves && !skeletonAppeared && skeletonGraveNumber == 0 && GravesExplored >= skeletonTrigger) ||
                ((page == StoryPage.SkeletonEmergence || page == StoryPage.SkeletonBattle) && (!skeletonAppeared || skeletonDefeated)) ||
                ((page == StoryPage.SkeletonLetter || page == StoryPage.SkeletonReward || page == StoryPage.Castle || page == StoryPage.SkeletonFinished) && !skeletonDefeated) ||
                (skeletonDefeated && (!skeletonAppeared || skeletonHealth <= 0)) ||
                (!skeletonDefeated && (skeletonHealth != 0 || skeletonScrolls != 0 || skeletonHorns != 0 || skeletonGuardianArmed)) ||
                (page == StoryPage.SkeletonReward && skeletonReward != ItemId.None) ||
                (goblinExperience && reward == ItemId.None) || (skeletonExperience && skeletonReward == ItemId.None) ||
                (skeletonReward != ItemId.None && !skeletonDefeated) || (castleSeen && skeletonReward == ItemId.None) ||
                ((page == StoryPage.Castle || page == StoryPage.SkeletonFinished) && skeletonReward == ItemId.None) ||
                (!hasCages && (cagesVisited != 0 || activeCage != -1 || bridgeFell || redKey || blueKey || flint || mirrorShield || amethystAmulet || knightDefeated)) ||
                ((int)page < (int)StoryPage.Bridge && (cagesVisited != 0 || redKey || blueKey || flint || mirrorShield || amethystAmulet || knightDefeated)) ||
                ((int)page >= (int)StoryPage.Bridge && (!hasCages || skeletonReward == ItemId.None)) ||
                (page == StoryPage.Anteroom && !knightDefeated) ||
                (anteroomTalked && !knightDefeated) ||
                (!knightDefeated && (knightHealth != 0 || knightScrolls != 0 || knightHorns != 0 || knightGuardianArmed || knightReward != ItemId.None || knightAttack != 0 || knightDefense != 0)) ||
                (!werewolfDefeated && (werewolfHealth != 0 || werewolfScrolls != 0 || werewolfHorns != 0 || werewolfGuardianArmed || werewolfReward != ItemId.None || werewolfAttack != 0 || werewolfDefense != 0)) ||
                (knightReward != ItemId.None && (!knightDefeated || knightHealth <= 0 || knightAttack <= 0)) ||
                (werewolfReward != ItemId.None && (!werewolfDefeated || werewolfHealth <= 0 || werewolfAttack <= 0)) ||
                (page == StoryPage.KnightBattle && knightDefeated) || (page == StoryPage.KnightReward && (!knightDefeated || knightHealth <= 0 || knightReward != ItemId.None)) ||
                (!knightDefeated && (cauldronsVisited != 0 || werewolfAppeared || werewolfDefeated || purpleKey || woodenStake || werewolfBurned || hallIntroSeen)) ||
                (cauldronsVisited != 0 && !redKey && !purpleKey) ||
                ((page == StoryPage.CarnalHall || page == StoryPage.Cauldron || page == StoryPage.WerewolfBattle ||
                    page == StoryPage.WerewolfReward || page == StoryPage.WerewolfEmergence) && (!knightDefeated || knightReward == ItemId.None || (!redKey && !purpleKey))) ||
                (page == StoryPage.Cauldron ? activeCauldron < 0 || (cauldronsVisited & (1 << activeCauldron)) == 0 : activeCauldron != -1) ||
                (werewolfDefeated && (!werewolfAppeared || werewolfHealth <= 0 || knightReward == ItemId.None)) || (werewolfBurned && (!werewolfDefeated || !flint)) ||
                (purpleKey && (redKey || (cauldronsVisited & (1 << 4)) == 0)) ||
                (woodenStake && (cauldronsVisited & (1 << 1)) == 0) ||
                ((page == StoryPage.WerewolfBattle || page == StoryPage.WerewolfReward || page == StoryPage.WerewolfEmergence) && !werewolfAppeared) ||
                (page == StoryPage.WerewolfEmergence && werewolfDefeated) ||
                (page == StoryPage.WerewolfBattle && werewolfDefeated) || (page == StoryPage.WerewolfReward && !werewolfDefeated) ||
                (page == StoryPage.WerewolfReward && werewolfReward != ItemId.None) ||
                (page == StoryPage.CarnalHall && werewolfDefeated && werewolfReward == ItemId.None) ||
                (page == StoryPage.BridgeCage ? activeCage < 0 || (cagesVisited & (1 << activeCage)) == 0 : activeCage != -1) ||
                (bridgeFell != (page == StoryPage.BridgeFall)) ||
                (redKey && (cagesVisited & (1 << (int)CageContent.RedKey)) == 0) ||
                (blueKey && (cagesVisited & (1 << (int)CageContent.BlueKey)) == 0) ||
                (flint && (cagesVisited & (1 << (int)CageContent.Flint)) == 0) ||
                (mirrorShield && (hero != CombatantId.Warrior || (cagesVisited & (1 << (int)CageContent.MirrorShield)) == 0)) ||
                (amethystAmulet && (hero != CombatantId.Mage || (cagesVisited & (1 << (int)CageContent.LetterAndAmulet)) == 0)))
                throw new ArgumentException("Inconsistent story checkpoint.");
        }
    }

}
