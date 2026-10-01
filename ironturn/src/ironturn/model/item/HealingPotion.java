package ironturn.model.item;

import ironturn.model.Character;

public class HealingPotion implements Item {

    @Override
    public String getName() {
        return "Poção de Cura";
    }

    @Override
    public String getDescription() {
        return "Recupera 40 HP";
    }

    @Override
    public void apply(Character target) {
        target.heal(40);
    }

}
