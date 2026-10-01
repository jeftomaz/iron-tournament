package ironturn;

import ironturn.battle.BattleEngine;
import ironturn.battle.UI;
import ironturn.model.HeroClass;

import java.util.Scanner;

public class GameMenu {

    private static final String SEP  = "═".repeat(40);
    private static final String THIN = "─".repeat(40);

    private final Scanner       scanner  = new Scanner(System.in);
    private final ProgressState progress = new ProgressState();   // ← novo

    // Loop principal

    public void run() {
        boolean running = true;
        while (running) {
            showTitle();
            int max = progress.isEnemyModeUnlocked() ? 4 : 3;    // ← dinâmico
            int choice = readChoice(max);
            switch (choice) {
                case 1 -> playGame();
                case 2 -> showHowToPlay();
                case 3 -> { running = false; showFarewell(); }
                case 4 -> playAsEnemy();                          // ← novo
            }
        }
    }

    // Ações do menu

    private void playGame() {
        BattleEngine engine = new BattleEngine(scanner);
        boolean won = engine.start();

        if (won) {
            progress.markCleared(engine.getHeroClass());
            progress.recordSnapshot(engine.getHeroClass(), engine.getHeroSnapshot());
            if (progress.isEnemyModeUnlocked() && !hadEnemyModeBeforeThisRun())
                showEnemyModeUnlocked();
        }

        System.out.println();
        System.out.println(UI.DIM + "  Pressione Enter para voltar ao menu..." + UI.RESET);
        scanner.nextLine();
    }

    private void playAsEnemy() {
        HeroClass[] classes = HeroClass.values();
        HeroClass chosen = classes[new java.util.Random().nextInt(classes.length)];
        BattleEngine engine = new BattleEngine(scanner, progress.getSnapshot(chosen));
        engine.start();

        System.out.println();
        System.out.println(UI.DIM + "  Pressione Enter para voltar ao menu..." + UI.RESET);
        scanner.nextLine();
    }

    private void showHowToPlay() {
        System.out.println();
        System.out.println(UI.BLUE + "  ╔" + SEP + "╗");
        System.out.println("  ║  COMO JOGAR" + " ".repeat(42 - 2 - 12) + "║");
        System.out.println("  ╚" + SEP + "╝" + UI.RESET);

        System.out.println();
        System.out.println("  Escolha uma classe e enfrente inimigos em sequência.");
        System.out.println("  A cada turno: Atacar · Defender · Ver Status");
        System.out.println();

        System.out.println(UI.YELLOW + "  " + THIN + UI.RESET);
        System.out.println(UI.YELLOW + "  CLASSES" + UI.RESET);
        System.out.println(UI.YELLOW + "  " + THIN + UI.RESET);
        System.out.println();
        System.out.printf("  %-12s  HP: 120  ATK: 20  DEF: 15%n", "Guerreiro");
        System.out.println("               Espada (+10 ATK) · Escudo (+8 DEF)");
        System.out.println("               Habilidade: crítico (5% chance, 2× dano)");
        System.out.println("               Defender: dano reflexivo com o escudo");
        System.out.println();
        System.out.printf("  %-12s  HP: 80   ATK: 15  DEF: 5%n",  "Mago");
        System.out.println("               Amuleto (+15 ATK, +30 HP máx)");
        System.out.println("               Habilidade: reverter 1 turno por inimigo");
        System.out.println();

        System.out.println(UI.YELLOW + "  " + THIN + UI.RESET);
        System.out.println(UI.YELLOW + "  INIMIGOS  (stats variam ±15% a cada partida)" + UI.RESET);
        System.out.println(UI.YELLOW + "  " + THIN + UI.RESET);
        System.out.println();
        System.out.printf("  %-14s  HP: ~40   ATK: ~15  DEF: ~3%n",  "Goblin");
        System.out.printf("  %-14s  HP: ~62   ATK: ~20  DEF: ~5%n",  "Esqueleto");
        System.out.printf("  %-14s  HP: ~80   ATK: ~26  DEF: ~8%n",  "Cavaleiro");
        System.out.printf("  %-14s  HP: ~95   ATK: ~30  DEF: ~10%n", "Lobisomem");
        System.out.printf("  %-14s  HP: ~110  ATK: ~34  DEF: ~12  ⚠ fúria%n", "Vampiro");
        System.out.printf("  %-14s  HP: ~125  ATK: ~38  DEF: ~14  ⚠ fúria%n", "Necromante");
        System.out.printf("  %-14s  HP: ~145  ATK: ~44  DEF: ~16  ⚠ fúria%n", "Rei Demônio");
        System.out.println();

        System.out.println(UI.DIM + "  Pressione Enter para voltar..." + UI.RESET);
        scanner.nextLine();
    }

    private void showFarewell() {
        System.out.println();
        System.out.println("  Até a próxima, aventureiro.");
        System.out.println();
    }

    // Renderização

    private void showTitle() {
        System.out.println();
        System.out.println("  ╔" + SEP + "╗");
        System.out.println("  ║" + " ".repeat(40) + "║");
        System.out.println("  ║   =[ I R O N   T U R N ]=              ║");
        System.out.println("  ║                                        ║");
        System.out.println("  ║   RPG por turnos · Java puro           ║");
        System.out.println("  ║                                        ║");
        System.out.println("  ╚" + SEP + "╝");
        System.out.println();
        System.out.println("  [1]  Jogar");
        System.out.println("  [2]  Como Jogar");
        System.out.println("  [3]  Sair");
        if (progress.isEnemyModeUnlocked())
            System.out.println("  [4]  " + UI.RED + "Jogar como Inimigo" + UI.RESET);   // ← condicional
        System.out.println();
        showProgressBadges();                                      // ← indicadores visuais
        System.out.println(UI.DIM + "  " + THIN + UI.RESET);
    }

    private void showProgressBadges() {
        if (!progress.isWarriorCleared() && !progress.isMageCleared()) return;
        System.out.print("  ");
        if (progress.isWarriorCleared()) System.out.print(UI.YELLOW + "∆ Guerreiro ✓  " + UI.RESET);
        if (progress.isMageCleared())    System.out.print(UI.BLUE   + "≈ Mago ✓"        + UI.RESET);
        System.out.println();
        System.out.println();
    }

    // Notificação de desbloqueio

    private boolean enemyModeWasUnlocked = false;               // ← estado anterior

    private boolean hadEnemyModeBeforeThisRun() {
        return enemyModeWasUnlocked;
    }

    private void showEnemyModeUnlocked() {
        enemyModeWasUnlocked = true;
        System.out.println();
        System.out.println(UI.RED + "  ╔" + SEP + "╗");
        System.out.println("  ║  ⚠  MODO INIMIGO DESBLOQUEADO!        ║");
        System.out.println("  ╚" + SEP + "╝" + UI.RESET);
        System.out.println();
        System.out.println("  Você zerou com as duas classes.");
        System.out.println("  A opção [4] agora está disponível no menu.");
        System.out.println();
    }

    // Input

    private int readChoice(int max) {
        while (true) {
            System.out.print("  > ");
            try {
                int n = Integer.parseInt(scanner.nextLine().trim());
                if (n >= 1 && n <= max) return n;
            } catch (NumberFormatException ignored) {}
            System.out.println("  Escolha entre 1 e " + max + ".");
        }
    }
}