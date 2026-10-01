package ironturn.pattern.observer;

import java.util.List;
import java.util.ArrayList;

public class BattleLogger implements BattleObserver {

    private List<String> log;

    public BattleLogger() {
        this.log = new ArrayList<>();
    }

    @Override
    public void onEvent(BattleEvent event) {
        String msg = event.getType() == BattleEvent.Type.GUARD
                ? String.format("%s levantou o escudo — causou %d de dano e dobrou sua defesa",
                event.getAttacker().getName(),
                event.getHitTaken())
                : String.format("%s atacou %s por %d de dano",
                event.getAttacker().getName(),
                event.getTarget().getName(),
                event.getHitTaken());
        log.add(msg);
    }

    public List<String> getLog() {
        return log;
    }
}
