package ironturn.pattern.strategy;

import ironturn.model.Character;

public interface AttackStrategy {

    int execute(Character attacker, Character target);

}
