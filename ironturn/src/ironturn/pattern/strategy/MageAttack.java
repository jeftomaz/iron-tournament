package ironturn.pattern.strategy;
import ironturn.model.Character;

public class MageAttack implements AttackStrategy {

    @Override
    public int execute(Character attacker, Character target) {
        return attacker.getAtk();
    }

}
