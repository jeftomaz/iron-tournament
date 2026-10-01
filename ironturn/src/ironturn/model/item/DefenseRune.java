package ironturn.model.item;

import ironturn.model.Character;

public class DefenseRune implements Item {
    @Override
    public String getName() {
        return "Runa do Guardião";
    }

    @Override
    public String getDescription() {
        return "+5 de DEF permanente";
    }

    @Override
    public void apply(Character target) {
        target.addDef(5);
    }
}
