package ironturn.pattern.decorator;

import ironturn.model.Character;

public class SwordDecorator extends CharacterDecorator {

    public SwordDecorator(Character wrapped) {
        super(wrapped);
    }

    @Override
    public int getAtk() { return wrapped.getAtk() + 10; }
}
