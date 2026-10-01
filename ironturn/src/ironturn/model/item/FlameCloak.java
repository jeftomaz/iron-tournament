package ironturn.model.item;

import ironturn.model.Hero;

public class FlameCloak implements Item {

    @Override public String getName() { return "Manto de Chamas"; }
    @Override public String getDescription() {
        return "As chamas não o consomem. Consomem o inimigo.";
    }

    @Override
    public void apply(ironturn.model.Character target) {
        if (target instanceof Hero h) h.setFlameCloakPending(true);
    }
}