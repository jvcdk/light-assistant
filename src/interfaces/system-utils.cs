namespace LightAssistant.Interfaces;

// Provides a set of system utils that need to be mocked out in unit tests
// such as DateTime.Now and ManualResetEvent.
internal interface ISystemUtils
{
    DateTime Now { get; }
    IManualResetEvent NewManualResetEvent(bool initialState);
}

internal interface IManualResetEvent
{
    void Set();
    void Reset();
    bool WaitOne(int millisecondsTimeout);
}
