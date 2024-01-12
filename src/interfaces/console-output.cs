namespace LightAssistant.Interfaces;

public interface IConsoleOutput
{
    void Info(string message);
    void InfoLine(string message);

    void Message(string message);
    void MessageLine(string message);

    void Error(string message);
    void ErrorLine(string message);
}
