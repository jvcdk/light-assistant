namespace LightAssistant.Utils;

public sealed class SlimReadWriteDataGuard<T>(T data) : IDisposable
{
    private readonly T _data = data;
    private readonly ReaderWriterLockSlim _lock = new();

    public void Dispose() => _lock.Dispose();

    public IDisposable ObtainReadLock(out T data)
    {
        data = _data;
        return new Guard(_lock, read: true);
    }

    public IDisposable ObtainWriteLock(out T data)
    {
        data = _data;
        return new Guard(_lock, read: false);
    }

    private class Guard : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock;
        private readonly bool _read;

        public Guard(ReaderWriterLockSlim @lock, bool read)
        {
            _lock = @lock;
            _read = read;
            if(read)
                _lock.EnterReadLock();
            else
                _lock.EnterWriteLock();
        }

        public void Dispose()
        {
            if(_read)
                _lock.ExitReadLock();
            else
                _lock.ExitWriteLock();
        }
    }
}
