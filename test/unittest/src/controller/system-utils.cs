namespace unittest.Controller;

using LightAssistant.Interfaces;

internal class SystemUtilsMock : ISystemUtils
{
    public DateTime Now { get; set; } = DateTime.Parse("2024-01-01T12:30:00"); // Fixed date-time for testing

    private List<ManualResetEvent> _mrEvents = [];

    public IManualResetEvent NewManualResetEvent(bool initialState)
    {
        var mre = new ManualResetEvent(initialState);
        _mrEvents.Add(mre);
        return mre;
    }

    internal void SimulateWaitOneTimeout()
    {
        foreach (var mre in _mrEvents)
            mre.SimulateWaitOneTimeout();
    }

    internal void SetNow(DateTime dateTime)
    {
        Now = dateTime;
    }

    private class ManualResetEvent(bool initialState) : IManualResetEvent, IDisposable
    {
        private readonly System.Threading.ManualResetEvent _backing = new(initialState);

        public void Set() => _backing.Set();

        public void Reset() => _backing.Reset();

        private System.Threading.ManualResetEvent _internalWait = new(false);

        public bool WaitOne(int millisecondsTimeout)
        {
            var idx = WaitHandle.WaitAny([_backing, _internalWait], millisecondsTimeout);
            if (idx == WaitHandle.WaitTimeout)
                return false;
            if (idx == 1)
                return false;
            return true;
        }

        internal void SimulateWaitOneTimeout() => _internalWait.Set();

        public void Dispose() => _backing.Dispose();
    }
}
