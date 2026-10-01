package ironturn.pattern.strategy;
import ironturn.model.Character;

import java.util.Random;

public class WarriorAttack implements AttackStrategy {

    private static final double CRIT_CHANCE = 0.07;
    private static final double PIERCE_CHANCE = 0.10;
    private static final double CRIT_MULTIPLIER = 2.0;

    private final Random random;

    public WarriorAttack() {
        this.random = new Random();
    }

    @Override
    public int execute(Character attacker, Character target) {
        int base = (random.nextDouble() < PIERCE_CHANCE)
                ? attacker.getAtk()
                : Math.max(0, attacker.getAtk() - target.getDef());

        if (random.nextDouble() < CRIT_CHANCE) {
            System.out.println("\n  ⚔  GOLPE CRÍTICO!");
            return (int)(base * CRIT_MULTIPLIER);
        }
        return base;
    }
}
