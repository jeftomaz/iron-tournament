package ironturn.model.item;

import ironturn.model.Character;
import ironturn.model.Hero;

public class BrotherhoodHorn implements Item {

    @Override public String getName() { return "Chifre da Irmandade"; }
    @Override public String getDescription() {
        return "Forjado no mesmo fogo. Partido ao meio.";
    }

    @Override
    public void apply(ironturn.model.Character target) {
        if (target instanceof Hero h) h.addGuardianDrop();
    }

}
