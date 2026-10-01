package ironturn.battle;

import ironturn.model.*;
import ironturn.model.Character;
import ironturn.model.item.HopeScroll;
import ironturn.model.item.Item;
import ironturn.pattern.command.AttackCommand;
import ironturn.pattern.command.CommandHistory;
import ironturn.pattern.command.GuardCommand;
import ironturn.pattern.decorator.*;
import ironturn.pattern.observer.BattleEvent;
import ironturn.pattern.observer.BattleLogger;
import ironturn.pattern.observer.BattleObserver;
import ironturn.pattern.observer.StatusDisplay;
import ironturn.pattern.strategy.AttackStrategy;
import ironturn.pattern.strategy.MageAttack;
import ironturn.pattern.strategy.WarriorAttack;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;
import java.util.Scanner;

import static ironturn.battle.UI.buildBar;

public class BattleEngine {

    private record MenuEntry(String label, Runnable action, boolean takesTurn) {}

    private Hero hero;
    private Character equipped;
    private List<Enemy> enemies;
    private Enemy currentEnemy;
    private CommandHistory history;
    private List<BattleObserver> observers;
    private Scanner scanner;
    private int heroHpSnapshot;
    private boolean enemyHasAttacked;
    private GuardCommand activeGuard;

    private boolean playAsEnemy = false;
    private HeroSnapshot heroBossSnapshot = null;
    private HeroSnapshot heroSnapshot = null;

    private boolean playerHasRaged = false;

    // Construtor do modo Herói
    public BattleEngine(Scanner scanner) {
        this.history = new CommandHistory();
        this.observers = new ArrayList<>();
        this.scanner = scanner;
        this.enemies = new ArrayList<>();
        this.activeGuard = null;
    }

    // Construtor do modo Inimigo
    public BattleEngine(Scanner scanner, HeroSnapshot heroBossSnapshot) {
        this(scanner);
        this.playAsEnemy      = true;
        this.heroBossSnapshot = heroBossSnapshot;
    }

    // Setup helpers

    public HeroClass getHeroClass() {
        return hero.getHeroClass();
    }
    public HeroSnapshot getHeroSnapshot() {
        return heroSnapshot;
    }

    private int vary(int base) {
        double factor = 0.85 + new Random().nextDouble() * 0.30;
        return (int)(base * factor);
    }

    private Hero createHero(String name, int type) {
        Hero h;
        switch (type) {
            case 1 -> {
                h = new Hero(name, 120, 120, 20, 15, new WarriorAttack(), HeroClass.WARRIOR);
                equipped = new ShieldDecorator(new SwordDecorator(h));
            }
            case 2 -> {
                h = new Hero(name, 80, 80, 15, 5, new MageAttack(), HeroClass.MAGE);
                equipped = new AmuletDecorator(h);
                h.setHp(equipped.getMaxHp());
            }
            default -> {
                System.out.println("Escolha inválida! Guerreiro selecionado por padrão.");
                h = new Hero(name, 120, 120, 20, 15, new WarriorAttack(), HeroClass.WARRIOR);
                equipped = new ShieldDecorator(new SwordDecorator(h));
            }
        }
        heroSnapshot = new HeroSnapshot(
                h.getName(),
                equipped.getMaxHp(),
                equipped.getAtk(),
                equipped.getDef()
        );
        return h;
    }

    private Enemy createEnemy(String name, int baseHp, int baseAtk, int baseDef, boolean enragable) {
        int hp = vary(baseHp);
        return new Enemy(name, hp, hp, vary(baseAtk), vary(baseDef), enragable);
    }

    private List<Enemy> createEnemies() {
        List<Enemy> list = new ArrayList<>();
        list.add(createEnemy("Goblin",          45, 15,  3, false));
        list.add(createEnemy("Esqueleto",       62, 20,  5, false));
        list.add(createEnemy("Cavaleiro",       80, 26,  8, false));
        list.add(createEnemy("Lobisomen",       95, 30, 10, false));
        list.add(createEnemy("Vampiro",        110, 34, 12, true));
        list.add(createEnemy("Necromante",     125, 38, 14,true));
        list.add(createEnemy("Rei Demônio",    145, 44, 16, true));
        return list;
    }

    private void setupEnemyMode() {
        UI.section("MODO INIMIGO");
        System.out.println();
        System.out.println("  Escolha seu personagem:");
        System.out.println();

        String[][] roster = {
                {"Goblin",       "45",  "15",  "3",  "warrior"},
                {"Esqueleto",    "62",  "20",  "5",  "warrior"},
                {"Cavaleiro",    "80",  "26",  "8",  "warrior"},
                {"Lobisomem",    "95",  "30", "10",  "warrior"},
                {"Vampiro",     "110",  "34", "12",  "mage"   },
                {"Necromante",  "125",  "38", "14",  "mage"   },
                {"Rei Demônio", "145",  "44", "16",  "warrior"},
        };

        for (int i = 0; i < roster.length; i++)
            System.out.printf("  [%d]  %-14s  HP: %s  | ATK: %s | DEF: %s%n",
                    i + 1, roster[i][0], roster[i][1], roster[i][2], roster[i][3]);

        System.out.println();
        System.out.print("  > ");
        int pick = Integer.parseInt(scanner.nextLine()) - 1;
        String[] chosen = roster[pick];

        AttackStrategy strategy = chosen[4].equals("mage")
                ? new MageAttack() : new WarriorAttack();

        int hp = Integer.parseInt(chosen[1]);
        hero     = new Hero(chosen[0], hp, hp,
                Integer.parseInt(chosen[2]),
                Integer.parseInt(chosen[3]),
                strategy, HeroClass.WARRIOR);
        equipped = hero;   // sem decorators

        enemies          = buildTieredEnemies(pick, roster);
        currentEnemy     = enemies.get(0);
        heroHpSnapshot   = hero.getHp();
        enemyHasAttacked = false;

        observers.add(new BattleLogger());
        observers.add(new StatusDisplay());

        System.out.println();
        System.out.printf("  Você é o %s. Prepare-se.%n", hero.getName());
        System.out.printf("  Primeiro inimigo: %s%n", currentEnemy.getName());
        UI.separator();
    }

    private List<Enemy> buildTieredEnemies(int chosenIndex, String[][] roster) {
        Random rng = new Random();
        int[][] tiers = {{0, 1}, {2, 3}, {4, 5}};  // índice 6 (Rei Demônio) excluído
        List<Enemy> list = new ArrayList<>();

        for (int[] tier : tiers) {
            List<Integer> options = new ArrayList<>();
            for (int idx : tier)
                if (idx != chosenIndex) options.add(idx);
            int idx = options.get(rng.nextInt(options.size()));
            String[] e = roster[idx];
            list.add(createEnemy(e[0],
                    Integer.parseInt(e[1]),
                    Integer.parseInt(e[2]),
                    Integer.parseInt(e[3]), false));
        }

        list.add(createHeroBoss());
        return list;
    }

    private Enemy createHeroBoss() {
        return new Enemy(
                heroBossSnapshot.name(),
                heroBossSnapshot.maxHp(),
                heroBossSnapshot.maxHp(),
                heroBossSnapshot.atk(),
                heroBossSnapshot.def(),
                false
        );
    }

    // UI helpers

    private void showEquipment() {
        System.out.println("  Equipamentos:");
        Character c = equipped;
        while (c instanceof CharacterDecorator dec) {
            if (c instanceof SwordDecorator)  System.out.println("    • Espada     +10 ATK");
            else if (c instanceof ShieldDecorator) System.out.println("    • Escudo      +8 DEF");
            else if (c instanceof AmuletDecorator) System.out.println("    • Amuleto     +15 ATK  +30 HP máx");
            else if (c instanceof FlameCloakDecorator)
                System.out.println("    • Manto de Chamas  +5 burn/ação");
            c = dec.getWrapped();
        }
    }

    private void showStatus() {
        System.out.println("\n--- STATUS ---");
        System.out.println("  " + buildBar(equipped));
        System.out.printf("  ATK: %d | DEF: %d%n", equipped.getAtk(), equipped.getDef());
        showEquipment();
        System.out.println();
        System.out.println("  " + buildBar(currentEnemy));
        System.out.printf("  ATK: %d | DEF: %d%n", currentEnemy.getAtk(), currentEnemy.getDef());
        System.out.println();
        if (hero.getHeroClass() == HeroClass.MAGE)
            System.out.println("  ✦ Feitiços de reversão: " + hero.getContraAvailable());
        if (hero.getHeroClass() == HeroClass.WARRIOR && hero.hasScroll())
            System.out.println("  ✦ Pergaminhos: " + hero.getScrollCount());
        if (hero.getHeroClass() == HeroClass.MAGE && hero.hasGuardianDrop())
            System.out.println("  ✦ Chifre da Irmandade: "
                    + (hero.isGuardianArmed() ? "ARMADO" : "disponível"));

        if (!hero.getInventory().isEmpty()) {
            System.out.println("  Itens coletados:");
            for (Item item : hero.getInventory())
                System.out.println("    • " + item.getName());
        }
    }

    // Menu dinâmico

    private List<MenuEntry> buildMenu() {
        List<MenuEntry> menu = new ArrayList<>();

        menu.add(new MenuEntry("Atacar", () -> {
            AttackCommand cmd = new AttackCommand(equipped, currentEnemy, observers);
            cmd.execute();
            history.push(cmd);
        }, true));

        if (hero.getHeroClass() == HeroClass.WARRIOR) {
            menu.add(new MenuEntry("Defender (levantar escudo)", () -> {
                activeGuard = new GuardCommand(equipped, currentEnemy, observers);
                activeGuard.execute();
            }, true));
        }

        double hpPct = (double) equipped.getHp() / equipped.getMaxHp();
        if (hero.getHeroClass() == HeroClass.WARRIOR
                && hero.hasScroll()
                && hpPct <= 0.30) {
            menu.add(new MenuEntry("Usar Pergaminho Misterioso", () -> {
                System.out.println("\n  ✦ Uma voz distante ecoa entre as sombras...");
                UI.pause(1500);
                System.out.println("  Um vulto surge — o Mago aparece por um breve instante.");
                UI.pause(2000);
                System.out.println("  \"Resista. Esse não é o seu fim.\"");
                UI.pause(3000);
                int healed = hero.useScroll();
                System.out.printf("  ✦ Uma luz quente restaura suas forças. (+%d HP)%n", healed);
            }, true));
        }

        if (hero.getHeroClass() == HeroClass.MAGE && hero.getContraAvailable() > 0) {
            if (enemyHasAttacked) {
                menu.add(new MenuEntry("Reverter Turno", () -> {
                    history.undo();
                    history.undo();
                    hero.useContra();
                    enemyHasAttacked = false;
                }, false));
            }
            menu.add(new MenuEntry("Reverter Batalha", () -> {
                hero.setHp(heroHpSnapshot);
                currentEnemy.setHp(currentEnemy.getMaxHp());
                history.clear();
                hero.useContra();
                enemyHasAttacked = false;
            }, false));
        }

        if (hero.getHeroClass() == HeroClass.MAGE
                && hero.hasGuardianDrop()
                && !hero.isGuardianArmed()) {
            menu.add(new MenuEntry("Preparar Proteção do Guerreiro", () -> {
                hero.activateGuardian();
                System.out.println("\n  ✦ O Guerreiro aguarda o seu chamado.");
            }, true));
        }

        menu.add(new MenuEntry("Ver Status (não gasta turno)", this::showStatus, false));

        return menu;
    }

    // Turn logic

    private void applyBurn() {
        int burn = equipped.getBurnDamage();
        if (burn <= 0 || !currentEnemy.isAlive()) return;
        currentEnemy.takeDamage(burn);
        System.out.println();
        System.out.println("    xXx Manto de Chamas queima " + currentEnemy.getName()
                + " por " + burn + " de dano!");
        System.out.println("    " + buildBar(currentEnemy));
        BattleEvent evt = new BattleEvent(equipped, currentEnemy, burn, BattleEvent.Type.BURN_DAMAGE);
        observers.forEach(o -> o.onEvent(evt));
    }

    private void triggerGuardianStrike() {
        int combinedDmg = HeroClass.WARRIOR.equippedAtk + equipped.getAtk();
        System.out.println("\n  ✦ O Chifre da Irmandade ressoa pelas sombras...");
        UI.pause(2000);
        System.out.println("  Uma figura surge — o Guerreiro, com sua espada erguida.");
        UI.pause(2000);
        System.out.printf("  Juntos, desferem um golpe combinado! (%d de dano)%n", combinedDmg);
        UI.pause(1500);
        currentEnemy.takeDamage(combinedDmg);
        BattleEvent evt = new BattleEvent(equipped, currentEnemy, combinedDmg);
        observers.forEach(o -> o.onEvent(evt));
        if (!currentEnemy.isAlive()) {
            int criticalHp = (int)(equipped.getMaxHp() * 0.30);
            int healed = criticalHp - hero.getHp();
            if (healed > 0) {
                hero.setHp(criticalHp);
                System.out.printf("  ✦ Uma energia te restaura. (+%d HP)%n", healed);
            }
        } else {
            hero.setHp(1);
            System.out.println("  O inimigo resistiu. Você sobreviveu por um fio.");
        }
    }

    private void playerTurn() {

        if (playAsEnemy && !playerHasRaged
                && (double) hero.getHp() / hero.getMaxHp() <= 0.30) {
            playerHasRaged = true;   // era: hero.triggerRage()
            UI.frameOpen(UI.RED, ">> FÚRIA — " + hero.getName());
            System.out.println("\n  ⚠  Você entra em fúria!");
            UI.pause(1500);
            int threshold = (int)(currentEnemy.getMaxHp() * 0.30);
            if (currentEnemy.getHp() > threshold) currentEnemy.setHp(threshold);
            System.out.println("  Um golpe devastador abala o inimigo.");
            UI.frameClose(UI.RED);
            return; // consome o turno
        }

        boolean actionTaken = false;

        System.out.println();
        System.out.println("Faça seu movimento!");
        UI.pause(2000);

        while (!actionTaken) {
            String title = ">> SEU TURNO :: " + hero.getName() + " vs " + currentEnemy.getName();
            UI.frameOpen(UI.BLUE, title);
            try {
                System.out.println();
                System.out.println("  " + buildBar(equipped));
                System.out.println("  " + buildBar(currentEnemy));
                System.out.println();
                UI.separator();

                List<MenuEntry> menu = buildMenu();
                for (int i = 0; i < menu.size(); i++)
                    System.out.println("  [" + (i + 1) + "] " + menu.get(i).label());

                UI.separator();
                System.out.print("  > ");

                int choice = Integer.parseInt(scanner.nextLine()) - 1;
                UI.markNewLine(); // ← sincroniza o estado do decorator após o Enter do usuário

                if (choice < 0 || choice >= menu.size()) {
                    System.out.println("  Comando inválido. Tente novamente.");
                    UI.frameClose(UI.BLUE);
                    continue;
                }

                MenuEntry entry = menu.get(choice);
                entry.action().run();
                if (entry.takesTurn()) { actionTaken = true; applyBurn(); }
                UI.frameClose(UI.BLUE);

            } catch (NumberFormatException e) {
                System.out.println("  Digite um número válido.");
                UI.frameClose(UI.BLUE);
            }
        }
    }

    private void enemyTurn() {
        UI.pause(1000);
        UI.frameOpen(UI.RED, "<<  TURNO DO INIMIGO  —  " + currentEnemy.getName());
        System.out.println();

        if (currentEnemy.canUseSpecial()) {
            System.out.println("  ⚠  " + currentEnemy.getName() + " entra em fúria!");
            UI.pause(1500);
            int hpBefore = hero.getHp();
            currentEnemy.triggerSpecial(equipped);
            System.out.println("  Um golpe devastador — você mal consegue se manter de pé.");
            if (!hero.isAlive() && hero.isGuardianArmed()) {
                hero.setHp(hpBefore);
                hero.disarmGuardian();
                triggerGuardianStrike();
            }
        } else {
            System.out.println("…O inimigo está se preparando para atacar!");
            UI.pause(3000);
            AttackCommand cmd = new AttackCommand(currentEnemy, equipped, observers);
            cmd.execute();
            if (!hero.isAlive() && hero.isGuardianArmed()) {
                cmd.undo();
                hero.disarmGuardian();
                triggerGuardianStrike();
            } else {
                history.push(cmd);
                enemyHasAttacked = true;
            }
        }

        applyBurn();
        UI.frameClose(UI.RED);
    }

    // Game flow

    private void setup() {
        if (playAsEnemy) { setupEnemyMode(); return; }

        System.out.println("  Digite o nome do seu personagem:");
        System.out.print("  > ");
        String name = scanner.nextLine();

        UI.section("ESCOLHA SUA CLASSE");
        System.out.println();
        System.out.println("  [1]  ∆ Guerreiro ∆ -- HP 120 | ATK 20 | DEF 15");
        System.out.println("       Espada (+10 ATK) e Escudo (+8 DEF)");
        System.out.println("       Habilidade: crítico e penetração de armadura");
        System.out.println();
        System.out.println("  [2]  ≈ Mago ≈ -- HP 80 | ATK 15 | DEF 5");
        System.out.println("       Amuleto (+15 ATK, +30 HP máx)");
        System.out.println("       Habilidade: reverter 1 turno por inimigo");
        System.out.println();
        System.out.print("  > ");
        int choice = Integer.parseInt(scanner.nextLine());

        hero             = createHero(name, choice);
        this.enemies     = createEnemies();
        currentEnemy     = enemies.get(0);
        heroHpSnapshot   = hero.getHp();
        enemyHasAttacked = false;

        observers.add(new BattleLogger());
        observers.add(new StatusDisplay());

        UI.separator();
        System.out.println();
        System.out.printf("  Bem-vindo(a), %s! Prepare-se para a batalha.%n", hero.getName());
        System.out.printf("  Primeiro inimigo: %s%n", currentEnemy.getName());
        UI.separator();
    }

    private Item showDropMenu(List<Item> drops) {
        UI.sectionDrop(">> SAQUE");
        System.out.println("O inimigo deixou cair alguns itens:\n");
        for (int i = 0; i < drops.size(); i++)
            System.out.printf(" [%d] %s -- %s%n", i + 1,
                    drops.get(i).getName(), drops.get(i).getDescription());
        System.out.println();
        UI.separator();

        int choice = -1;
        while (choice < 1 || choice > drops.size()) {
            try {
                System.out.print(" > ");
                choice = Integer.parseInt(scanner.nextLine());
                if (choice < 1 || choice > drops.size())
                    System.out.println(" Escolha entre 1 à " + drops.size() + ".");
            } catch (NumberFormatException e) {
                System.out.println(" Digite um número válido");
            }
        }
        return drops.get(choice - 1);
    }

    /** Trata morte do inimigo, drops e transição para o próximo. Retorna true se houve morte. */
    private boolean handleEnemyDefeat() {
        if (currentEnemy.isAlive()) return false;

        UI.enemyDefeated(currentEnemy.getName());

        if (activeGuard != null) {
            activeGuard.revert();
            activeGuard = null;
        }

        enemies.remove(currentEnemy);
        if (!enemies.isEmpty()) {
            if (!playAsEnemy) {
                List<Item> drops = DropTable.roll(hero.getHeroClass());
                Item chosen = showDropMenu(drops);
                chosen.apply(hero);
                if (hero.hasFlameCloakPending()) {
                    equipped = new FlameCloakDecorator(equipped);
                    hero.setFlameCloakPending(false);
                }
                if (!(chosen instanceof HopeScroll)) hero.addToInventory(chosen);
                System.out.println(UI.GREEN + " Você obteve: " + chosen.getName() + "!" + UI.RESET);
                UI.separator();
            } else if (enemies.size() == 1) {
                List<Item> drops = DropTable.rollBase(3);
                Item chosen = showDropMenu(drops);
                chosen.apply(hero);
                System.out.println(UI.GREEN + " Você obteve: " + chosen.getName() + "!" + UI.RESET);
                UI.separator();
            }
            currentEnemy = enemies.get(0);
            heroHpSnapshot = hero.getHp();
            enemyHasAttacked = false;
            playerHasRaged = false;
            activeGuard = null;
            hero.resetContra();
            UI.separator();
            System.out.println("  [!] Um novo inimigo surge das sombras: " + currentEnemy.getName() + "!");
            UI.pause(3000);
            UI.separator();
        }
        return true;
    }

    private void loop() {
        while (hero.isAlive() && !enemies.isEmpty()) {
            playerTurn();
            if (handleEnemyDefeat()) continue;

            enemyTurn();
            if (handleEnemyDefeat()) continue;   // ← FIX Bug 1: detecta morte por guardian strike

            if (activeGuard != null) {
                activeGuard.revert();
                activeGuard = null;
            }
            UI.turnDivider();
        }
    }

    private boolean ending() {
        UI.blank();
        if (hero.isAlive()) {
            if (playAsEnemy) {
                UI.sectionEnemy("  ═══  VITÓRIA  ═══");
                UI.blank();
                System.out.println("  As sombras retornam... por ora.");
                UI.blank();
                System.out.printf("  %s dominou o campo de batalha.%n", hero.getName());
            } else {
                UI.section("  ═══  VITÓRIA  ═══");
                UI.blank();
                System.out.println("  O Rei Demônio se desfaz em pó.");
                System.out.println("  A escuridão se dissipa — por ora.");
                UI.blank();
                System.out.printf("  %s, seu nome será lembrado.%n", hero.getName());
            }
        } else {
            UI.sectionEnemy("  ═══  FIM DE JOGO  ═══");
            UI.blank();
            System.out.println("  A escuridão não foi contida.");
            System.out.printf("  %s caiu, mas a luta não termina aqui.%n", hero.getName());
        }
        UI.blank();
        return hero.isAlive();
    }

    public boolean start() {
        setup();
        loop();
        return ending();
    }
}