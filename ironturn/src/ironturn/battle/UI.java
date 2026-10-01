package ironturn.battle;

import ironturn.model.Character;

import java.io.FilterOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.io.PrintStream;
import java.nio.charset.StandardCharsets;

public class UI {

    private static final int    WIDTH = 40;
    private static final String SEP   = "═".repeat(WIDTH);
    private static final String THIN  = "─".repeat(WIDTH);

    // ANSI
    public static final String RESET  = "\u001B[0m";
    public static final String BLUE   = "\u001B[94m";  // turno do herói
    public static final String RED    = "\u001B[91m";  // turno do inimigo / HP baixo / morte
    public static final String YELLOW = "\u001B[93m";  // transição / crítico
    public static final String GREEN  = "\u001B[92m";  // drop / HP normal
    public static final String DIM    = "\u001B[2m";   // separadores

    // Stream original capturado na carga da classe — nunca substitui pelo frame
    private static final PrintStream BASE_OUT = System.out;

    // Referência ao frame ativo (null quando fora de um frame)
    private static FramedOutputStream currentFrame = null;

    // ─── Decorator de stream ─────────────────────────────────────────────────
    //
    // Aplica o padrão Decorator ao OutputStream, da mesma forma que
    // BufferedOutputStream e FilterOutputStream funcionam na própria JDK.
    //
    // Cada byte escrito ao System.out passa por write(int b):
    //   • Se estamos no início de uma nova linha e o byte não é '\n',
    //     escreve o prefixo "║  " antes do conteúdo.
    //   • Linhas vazias ('\n' quando atLineStart==true) recebem só "║".
    //   • Qualquer outro byte é repassado diretamente ao stream base.
    //
    // Nenhum observer, command ou lógica de batalha precisa saber
    // que está dentro de um frame — o decorator é transparente.

    private static final class FramedOutputStream extends FilterOutputStream {

        private final byte[] prefixBytes;     // "║  " colorido
        private final byte[] emptyLineBytes;  // "║" colorido + '\n'
        boolean atLineStart = true;           // package-visible: UI.markNewLine() acessa

        FramedOutputStream(OutputStream wrapped, String color) {
            super(wrapped);
            this.prefixBytes    = (color + "  ║  " + RESET).getBytes(StandardCharsets.UTF_8);
            this.emptyLineBytes = (color + "  ║"   + RESET + "\n").getBytes(StandardCharsets.UTF_8);
        }

        @Override
        public void write(int b) throws IOException {
            if (atLineStart) {
                if (b == '\n') {
                    // Linha vazia: exibe apenas "║" e mantém atLineStart=true
                    out.write(emptyLineBytes);
                    return;
                }
                out.write(prefixBytes);
                atLineStart = false;
            }
            out.write(b);
            if (b == '\n') atLineStart = true;
        }

        @Override
        public void write(byte[] b, int off, int len) throws IOException {
            // Delega byte a byte para que write(int) aplique a lógica de prefixo
            for (int i = off; i < off + len; i++) write(b[i] & 0xFF);
        }

        @Override
        public void flush() throws IOException { out.flush(); }
    }

    // ─── API pública de frames ────────────────────────────────────────────────

    /**
     * Abre um frame colorido com título.
     * Redireciona System.out pelo FramedOutputStream até frameClose().
     * Todo output subsequente (observers, commands, etc.) é enquadrado
     * automaticamente, sem necessidade de modificar essas classes.
     *
     * Estrutura resultante:
     *   ╔═══════════════════════════════════════╗
     *   ║  <título>                              ║
     *   ╠═══════════════════════════════════════╣   ← separador interno
     *   ║  conteúdo...
     *   ╚═══════════════════════════════════════╝   ← fechado por frameClose()
     */
    public static void frameOpen(String color, String title) {
        BASE_OUT.println();
        BASE_OUT.println(color + "  ╔" + SEP + "╗");
        BASE_OUT.println(color + "  ║  " + title + sp(WIDTH - 2 - visLen(title)) + "║");
        BASE_OUT.println(color + "  ╠" + SEP + "╣" + RESET);
        currentFrame = new FramedOutputStream(BASE_OUT, color);
        System.setOut(new PrintStream(currentFrame, true, StandardCharsets.UTF_8));
    }

    /**
     * Fecha o frame ativo, restaura System.out e imprime a borda inferior.
     */
    public static void frameClose(String color) {
        System.out.flush();
        currentFrame = null;
        System.setOut(BASE_OUT);
        BASE_OUT.println(color + "  ╚" + SEP + "╝" + RESET);
    }

    /**
     * Sincroniza o estado do decorator após leitura de stdin.
     *
     * Problema: System.out.print("  > ") deixa atLineStart=false.
     * Quando o jogador pressiona Enter, o terminal avança o cursor para
     * a próxima linha — mas esse '\n' não passa pelo nosso OutputStream.
     * Sem esse ajuste, a próxima linha de output não receberia o prefixo "║".
     *
     * Deve ser chamado imediatamente após scanner.nextLine().
     */
    public static void markNewLine() {
        if (currentFrame != null) currentFrame.atLineStart = true;
    }

    // ─── Helpers de seção (sem frames) ───────────────────────────────────────

    public static void titleScreen() {
        blank();
        BASE_OUT.println("  ╔" + SEP + "╗");
        BASE_OUT.println("  ║" + sp(WIDTH) + "║");
        BASE_OUT.println("  ║" + pad("=[ I R O N   T U R N ]=", WIDTH) + "║");
        BASE_OUT.println("  ║" + sp(WIDTH) + "║");
        BASE_OUT.println("  ║" + pad("RPG por turnos · Java puro", WIDTH) + "║");
        BASE_OUT.println("  ║" + sp(WIDTH) + "║");
        BASE_OUT.println("  ╚" + SEP + "╝");
        blank();
    }

    public static void section(String title) {
        blank();
        System.out.println(BLUE +
                "  ╔" + SEP + "╗\n" +
                "  ║  " + title + sp(WIDTH - 2 - visLen(title)) + "║\n" +
                "  ╚" + SEP + "╝" + RESET);
    }

    public static void sectionEnemy(String title) {
        blank();
        System.out.println(RED +
                "  ╔" + SEP + "╗\n" +
                "  ║  " + title + sp(WIDTH - 2 - visLen(title)) + "║\n" +
                "  ╚" + SEP + "╝" + RESET);
    }

    public static void sectionDrop(String title) {
        blank();
        System.out.println(GREEN +
                "  ╔" + SEP + "╗\n" +
                "  ║  " + title + sp(WIDTH - 2 - visLen(title)) + "║\n" +
                "  ╚" + SEP + "╝" + RESET);
    }

    public static void turnDivider() {
        blank();
        System.out.println(YELLOW + "  " + "━".repeat(WIDTH) + RESET);
        blank();
    }

    public static void separator() {
        System.out.println(DIM + "  " + THIN + RESET);
    }

    public static void enemyDefeated(String name) {
        blank();
        System.out.println(RED + "  [+] " + name + " foi derrotado!" + RESET);
    }

    public static void blank() { System.out.println(); }

    // ─── Utilidades internas ──────────────────────────────────────────────────

    private static String pad(String text, int width) {
        String s = "   " + text;
        return s + sp(Math.max(0, width - visLen(s)));
    }

    private static String sp(int n) { return n > 0 ? " ".repeat(n) : ""; }

    /**
     * Comprimento visual da string, ignorando sequências ANSI.
     * Necessário para alinhar bordas dos frames corretamente.
     */
    private static int visLen(String s) {
        // Remove sequências ANSI antes de calcular o comprimento
        String stripped = s.replaceAll("\u001B\\[[0-9;]*m", "");
        int len = 0;
        for (int i = 0; i < stripped.length(); ) {
            int cp = stripped.codePointAt(i);
            len += (cp > 0xFFFF) ? 2 : 1;
            i += java.lang.Character.charCount(cp);
        }
        return len;
    }

    public static void pause(int ms) {
        try {
            Thread.sleep(ms);
            int avail = System.in.available();
            if (avail > 0) System.in.read(new byte[avail]);
        } catch (Exception ignored) {}
    }

    public static String buildBar(Character c, int damage) {
        int barSize = 10;
        int filled  = (int)((double) c.getHp() / c.getMaxHp() * barSize);
        int empty   = barSize - filled;
        double pct  = (double) c.getHp() / c.getMaxHp();

        String barColor = pct <= 0.30 ? RED : GREEN;
        String dmgStr   = damage > 0 ? RED + "  (-" + damage + ")" + RESET : "";

        return String.format("%s [%s%s%s%s] %d/%d HP%s",
                c.getName(),
                barColor, "█".repeat(filled), "░".repeat(empty), RESET,
                c.getHp(), c.getMaxHp(),
                dmgStr);
    }

    public static String buildBar(Character c) { return buildBar(c, 0); }
}