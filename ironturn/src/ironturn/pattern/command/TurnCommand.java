package ironturn.pattern.command;

public interface TurnCommand {

    void execute();
    void undo();

}
