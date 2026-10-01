package ironturn.model.item;

import ironturn.model.Character;

public class DivineDefense implements Item {
    @Override
    public String getName() {
        return "Defesa Divina";
    }

    @Override
    public String getDescription() {
        return "+15 de DEF permanente";
    }

    @Override
    public void apply(Character target) {
        target.addDef(15);
    }
}
