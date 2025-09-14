using LightAssistant;
using LightAssistant.Interfaces;

namespace unittest;

internal class AssertingConsoleOutput : IConsoleOutput
{
    private ConsoleOutput _inner = new();

    public void Error(string message)
    {
        _inner.Error(message);
        Assert.Fail(message);
    }

    public void ErrorLine(string message)
    {
        _inner.ErrorLine(message);
        Assert.Fail(message);
    }

    public void Info(string message) => _inner.Info(message);
    public void InfoLine(string message) => _inner.InfoLine(message);
    public void Message(string message) => _inner.Message(message);
    public void MessageLine(string message) => _inner.MessageLine(message);
}