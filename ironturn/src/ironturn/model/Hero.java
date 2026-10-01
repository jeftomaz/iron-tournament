package ironturn.model;
import ironturn.model.item.Item;
import ironturn.pattern.strategy.AttackStrategy;

import java.util.ArrayList;
import java.util.List;

public class Hero extends Character{

    private AttackStrategy strategy;
    private final HeroClass heroClass;
    private int undosRemaining;
    private int scrollCount = 0;
    private final List<Item> inventory = new ArrayList<>();

    // Variáveis para o Chifre da Irmandade
    private int guardianDropCount = 0;
    private boolean guardianArmed = false;
    public boolean hasGuardianDrop() { return guardianDropCount > 0; }
    public boolean isGuardianArmed() { return guardianArmed; }
    public void addGuardianDrop() { guardianDropCount++; }
    public void disarmGuardian() { guardianArmed = false; }

    // Variáveis para o Manto de Chamas
    private boolean flameCloakPending = false;
    public boolean hasFlameCloakPending()       { return flameCloakPending; }
    public void setFlameCloakPending(boolean v) { flameCloakPending = v; }

    public Hero(String name, int hp, int hpMax, int atk, int def, AttackStrategy strategy, HeroClass heroClass) {
        super(name, hp, hpMax, atk, def);
        this.strategy = strategy;
        this.heroClass = heroClass;
        this.undosRemaining = (heroClass == HeroClass.MAGE) ? 1 : 0;
    }

    public HeroClass getHeroClass() { return heroClass; }

    public int getContraAvailable() { return undosRemaining; }
    public void useContra() { if (undosRemaining > 0) undosRemaining-- ;}
    public void resetContra() { undosRemaining = 1; }

    public boolean hasScroll() { return scrollCount > 0; }
    public int getScrollCount() { return scrollCount; }
    public void addScroll() { scrollCount++; }

    @Override
    public int attack(Character target) {
        return strategy.execute(this, target);
    }

    public int useScroll() {
        if (scrollCount == 0) return 0;
        scrollCount--;
        int amount = (int)(getMaxHp() * 0.5);
        heal(amount);
        return amount;
    }

    public boolean activateGuardian() {
        if (guardianDropCount == 0) return false;
        guardianDropCount--;
        guardianArmed = true;
        return true;
    }

    public void addToInventory(Item item) { inventory.add(item); }
    public List<Item> getInventory() { return inventory; }
}