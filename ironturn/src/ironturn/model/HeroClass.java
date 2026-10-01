package ironturn.model;

public enum HeroClass {
    WARRIOR(30), MAGE(35);

    public final int equippedAtk;
    HeroClass(int equippedAtk) { this.equippedAtk = equippedAtk; }
}
