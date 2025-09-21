namespace LightAssistant;

using Interfaces;

internal class SystemUtils : ISystemUtils
{
    public DateTime Now => DateTime.Now;

    public IManualResetEvent NewManualResetEvent(bool initialState) =>
        new ManualResetEvent(initialState);

    private class ManualResetEvent(bool initialState) : IManualResetEvent, IDisposable
    {
        private readonly System.Threading.ManualResetEvent _backing = new(initialState);

        public void Set() => _backing.Set();

        public void Reset() => _backing.Reset();

        public bool WaitOne(int millisecondsTimeout) => _backing.WaitOne(millisecondsTimeout);

        public void Dispose() => _backing.Dispose();
    }
}

