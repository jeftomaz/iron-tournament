package ironturn.pattern.decorator;

import ironturn.model.Character;

public class ShieldDecorator extends CharacterDecorator {

    public ShieldDecorator(Character wrapped) { super(wrapped); }

    @Override
    public int getDef() { return wrapped.getDef() + 8; }
}
