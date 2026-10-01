package ironturn.pattern.observer;

import ironturn.battle.UI;
import ironturn.model.Character;
import ironturn.model.Hero;

public class StatusDisplay implements BattleObserver {

    @Override
    public void onEvent(BattleEvent event) {

        if (event.getType() == BattleEvent.Type.BURN_DAMAGE) return;

        Character hero  = event.getAttacker() instanceof Hero ? event.getAttacker() : event.getTarget();
        Character enemy = event.getAttacker() instanceof Hero ? event.getTarget() : event.getAttacker();

        int heroDmg  = event.getTarget() == hero ? event.getHitTaken() : 0;
        int enemyDmg = event.getTarget() == enemy ? event.getHitTaken() : 0;

        System.out.println();
        if (event.getType() == BattleEvent.Type.GUARD) {
            System.out.printf("  ** %s levanta o escudo e causa %d de dano reflexivo!%n",
                    event.getAttacker().getName(), event.getHitTaken());
        } else {
            System.out.printf("  ** %s ataca %s por %d de dano!%n",
                    event.getAttacker().getName(), event.getTarget().getName(), event.getHitTaken());
        }
        UI.separator();
        System.out.println("  " + UI.buildBar(hero,  heroDmg));
        System.out.println("  " + UI.buildBar(enemy, enemyDmg));
    }
}
