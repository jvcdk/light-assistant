namespace LightAssistant.Utils;

public class DataGuard<T>
{
    private T _data;
    private Mutex _guard = new();

    public DataGuard(T data)
    {
        _data = data;
    }

    public IDisposable ObtainLock(out T data)
    {
        _guard.WaitOne();
        data = _data;
        return new LockGuard(Release);
    }

    private void Release() => _guard.ReleaseMutex();

    private class LockGuard(Action release) : IDisposable
    {
        private readonly Action _release = release;
        public void Dispose() => _release();
    }
}
