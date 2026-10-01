package ironturn.pattern.observer;

import ironturn.model.Character;

public class BattleEvent {

    public enum Type { ATTACK, GUARD, BURN_DAMAGE }

    private final Character attacker;
    private final Character target;
    private final int hitTaken;
    private final Type type;

    public BattleEvent(Character attacker, Character target, int hitTaken) {
        this(attacker, target, hitTaken, Type.ATTACK);
    }

    public BattleEvent(Character attacker, Character target, int hitTaken, Type type) {
        this.attacker = attacker;
        this.target = target;
        this.hitTaken = hitTaken;
        this.type = type;
    }

    public Character getAttacker() { return attacker; }
    public Character getTarget() { return target; }
    public int getHitTaken() { return hitTaken; }
    public Type getType() { return type; }
}
