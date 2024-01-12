using LightAssistant.Interfaces;

namespace LightAssistant;

internal class ConsoleOutput : IConsoleOutput
{
    public bool Verbose { get; set; } = false;

    public void Error(string message)
    {
        Console.Error.Write(message);
    }

    public void ErrorLine(string message)
    {
        Error(message + Environment.NewLine);
    }

    public void Info(string message)
    {
        if(Verbose)
            Message(message);
    }

    public void InfoLine(string message)
    {
        Info(message + Environment.NewLine);
    }

    public void Message(string message)
    {
        Console.Write(message);
    }

    public void MessageLine(string message)
    {
         Message(message + Environment.NewLine);
   }
}