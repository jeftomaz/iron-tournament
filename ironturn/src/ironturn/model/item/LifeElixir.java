package ironturn.model.item;

import ironturn.model.Character;

public class LifeElixir implements Item {

    @Override
    public String getName() {
        return "Elixir da Vida";
    }

    @Override
    public String getDescription() {
        return "Recupera a saúde completa";
    }

    @Override
    public void apply(Character target) {
        target.heal(target.getMaxHp());
    }
}
