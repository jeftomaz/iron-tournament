package ironturn.model.item;

import ironturn.model.Character;

public class AttackGem implements Item {
    @Override
    public String getName() {
        return "Gema de Sangue";
    }

    @Override
    public String getDescription() {
        return "+10 de ATK permanente";
    }

    @Override
    public void apply(Character target) {
        target.addAtk(10);
    }
}
