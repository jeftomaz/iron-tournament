package ironturn.model.item;

import ironturn.model.Character;

public class PowerCrystal implements Item {
    @Override
    public String getName() {
        return "Cristal da Ruína";
    }

    @Override
    public String getDescription() {
        return "+15 de ATK permanente";
    }

    @Override
    public void apply(Character target) {
        target.addAtk(15);
    }
}
