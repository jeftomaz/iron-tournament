package ironturn;

import ironturn.model.HeroSnapshot;
import ironturn.model.HeroClass;

public class ProgressState {

    private boolean       warriorCleared       = false;
    private boolean       mageCleared          = false;
    private HeroSnapshot  lastWarriorSnapshot  = null;
    private HeroSnapshot  lastMageSnapshot     = null;

    public void markCleared(HeroClass heroClass) {
        if (heroClass == HeroClass.WARRIOR) warriorCleared = true;
        else if (heroClass == HeroClass.MAGE) mageCleared = true;
    }

    public void recordSnapshot(HeroClass heroClass, HeroSnapshot snapshot) {
        if (heroClass == HeroClass.WARRIOR) lastWarriorSnapshot = snapshot;
        else if (heroClass == HeroClass.MAGE) lastMageSnapshot = snapshot;
    }

    public HeroSnapshot getSnapshot(HeroClass heroClass) {
        if (heroClass == HeroClass.WARRIOR)
            return lastWarriorSnapshot != null
                    ? lastWarriorSnapshot
                    : new HeroSnapshot("Guerreiro Lendário", 120, 30, 23);
        return lastMageSnapshot != null
                ? lastMageSnapshot
                : new HeroSnapshot("Mago Ancestral", 110, 35, 5);
    }

    public boolean isWarriorCleared() { return warriorCleared; }
    public boolean isMageCleared()    { return mageCleared;    }

    public boolean isEnemyModeUnlocked() {
        return warriorCleared && mageCleared;
    }
}