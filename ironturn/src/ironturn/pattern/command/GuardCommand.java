package ironturn.pattern.command;

import ironturn.model.Character;
import ironturn.model.Hero;
import ironturn.pattern.observer.BattleEvent;
import ironturn.pattern.observer.BattleObserver;

import java.util.List;

public class GuardCommand {

    private final Character hero;
    private final Character enemy;
    private final List<BattleObserver> observers;
    private int defBonus;

    public GuardCommand(Character hero, Character enemy, List<BattleObserver> observers) {
        this.hero = hero;
        this.enemy = enemy;
        this.observers = observers;
    }

    public void execute() {
        defBonus = hero.getDef();
        hero.addDef(defBonus);

        int shieldDmg = Math.max(1, defBonus / 3);
        enemy.takeDamage(shieldDmg);

        for (BattleObserver o : observers)
            o.onEvent(new BattleEvent(hero, enemy, shieldDmg, BattleEvent.Type.GUARD));
    }

    public void revert() {
        hero.addDef(-defBonus);
    }
}
