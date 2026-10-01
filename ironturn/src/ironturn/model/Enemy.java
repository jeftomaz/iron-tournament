package ironturn.model;
import java.util.Random;

public class Enemy extends Character{

    private static final double ATTACK_VARIANCE = 0.2;
    private final Random random;
    private boolean hasUsedSpecial = false;
    private final boolean enrageable;

    public Enemy(String name, int hp, int hpMax, int atk, int def, boolean enrageable) {
        super(name, hp, hpMax, atk, def);
        this.random = new Random();
        this.enrageable = enrageable;
    }

    public boolean canUseSpecial() {
        return enrageable && !hasUsedSpecial && (double) getHp() / getMaxHp() <= 0.30;
    }

    public void triggerSpecial(Character target) {
        hasUsedSpecial = true;
        int threshold = (int)(target.getMaxHp() * 0.30);
        if (target.getHp() > threshold) target.setHp(threshold);
    }

    @Override
    public int attack(Character target) {
        int min = (int)(getAtk() * (1 - ATTACK_VARIANCE));
        int max = (int)(getAtk() * (1 + ATTACK_VARIANCE));
        int raw = random.nextInt(min, max + 1);
        return Math.max(0, raw - target.getDef());
    }
}