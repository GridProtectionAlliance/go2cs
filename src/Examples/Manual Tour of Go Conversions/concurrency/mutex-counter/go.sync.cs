using System.Threading;

namespace go
{
    public static class sync_package
    {
        public struct Mutex {
            private object _lock;
            public object Lock => _lock ??= new object();
        }

        public static void Lock(this ref Mutex mutex) => Monitor.Enter(mutex.Lock);

        public static void Unlock(this ref Mutex mutex) => Monitor.Exit(mutex.Lock);
    }
}
