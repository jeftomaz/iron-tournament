package ironturn.pattern.decorator;

import ironturn.model.Character;

public class AmuletDecorator extends CharacterDecorator {

    public AmuletDecorator(Character wrapped) {
        super(wrapped);
        wrapped.addMaxHp(30);
        wrapped.addAtk(15);
    }

    // @Override
    // public int getMaxHp() { return wrapped.getMaxHp() + 30; }

    // @Override
    // public int getAtk() { return wrapped.getAtk() + 5; }
}
