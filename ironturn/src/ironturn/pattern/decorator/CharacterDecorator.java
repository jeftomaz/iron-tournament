package ironturn.pattern.decorator;

import ironturn.model.Character;

public abstract class CharacterDecorator extends Character {

    protected final Character wrapped;

    public CharacterDecorator(Character wrapped) {
        super(wrapped.getName(), wrapped.getHp(), wrapped.getMaxHp(), wrapped.getAtk(), wrapped.getDef());
        this.wrapped = wrapped;
    }

    // Sobreescrita de métodos de Character
    @Override
    public int getHp() { return wrapped.getHp(); }

    @Override
    public int getMaxHp() { return wrapped.getMaxHp(); }

    @Override
    public int getAtk() { return wrapped.getAtk(); }

    @Override
    public int getDef() { return wrapped.getDef(); }

    @Override
    public void takeDamage(int hitTaken) { wrapped.takeDamage(hitTaken); }

    @Override
    public boolean isAlive() { return wrapped.isAlive(); }

    @Override
    public int attack(Character target) { return wrapped.attack(target); }

    @Override
    public void heal(int amount)       { wrapped.heal(amount); }

    @Override
    public void setHp(int amount)      { wrapped.setHp(amount); }

    @Override
    public void addDef(int amount)     { wrapped.addDef(amount); }

    @Override
    public void addAtk(int amount)     { wrapped.addAtk(amount); }

    @Override
    public void addMaxHp(int amount)   { wrapped.addMaxHp(amount); }

    public Character getWrapped() { return wrapped; }

}
