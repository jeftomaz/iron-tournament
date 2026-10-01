package ironturn.pattern.command;

import ironturn.model.Character;
import ironturn.pattern.observer.BattleEvent;
import ironturn.pattern.observer.BattleObserver;

import java.util.List;

public class AttackCommand implements TurnCommand {

    private final Character attacker;
    private Character target;
    private final List<BattleObserver> observers;
    private int damageDealt;

    public AttackCommand(Character attacker, Character target, List<BattleObserver> observers) {
        this.attacker = attacker;
        this.target = target;
        this.observers = observers;
        this.damageDealt = 0;
    }

    @Override
    public void execute() {
        damageDealt = attacker.attack(target);
        target.takeDamage(damageDealt);

        BattleEvent event = new BattleEvent(attacker, target, damageDealt);
        for (BattleObserver observer : observers) {
            observer.onEvent(event);
        }
    }

    @Override
    public void undo() {
        target.heal(damageDealt);

        System.out.println("✦ " + attacker.getName() + " reverte o tempo - o dano foi desfeito.");
    }
}
