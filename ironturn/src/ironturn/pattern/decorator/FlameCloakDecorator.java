package ironturn.pattern.decorator;

import ironturn.model.Character;

public class FlameCloakDecorator extends CharacterDecorator {

    private static final int BURN = 5;

    public FlameCloakDecorator(Character wrapped) {
        super(wrapped);
    }

    @Override
    public int getBurnDamage() { return BURN; }
}