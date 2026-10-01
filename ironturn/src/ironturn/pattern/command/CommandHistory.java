package ironturn.pattern.command;

import java.util.Stack;

public class CommandHistory {

    private final Stack<TurnCommand> history = new Stack<>();

    public void push(TurnCommand command) {
        history.push(command);
    }

    public void undo(){
        if (history.isEmpty()) {
            System.out.println("Nenhuma ação para desfazer.");
            return;
        }
        TurnCommand last = history.pop();
        last.undo();
    }

    public boolean isEmpty() {
        return history.isEmpty();
    }

    public void clear() {
        history.clear();
    }
}
