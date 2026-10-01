package ironturn.model.item;
import ironturn.model.Character;
import ironturn.model.Hero;

public class HopeScroll implements Item {

    @Override public String getName() { return "Pergaminho Misterioso"; }

    @Override public String getDescription() {
        return "\"Frater, ubi umbra te vincit — lux mea te quaeret.\"";
    }

    @Override
    public void apply(Character target) {
        if (target instanceof Hero h) h.addScroll();
    }
}