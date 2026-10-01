package ironturn.model.item;

import ironturn.model.Character;

public interface Item {
    String getName();
    String getDescription();
    void apply(Character target);
}
