namespace Vayosoft.Utilities
{
    //https://stackoverflow.com/questions/31138179/asynchronous-locking-based-on-a-key
    //Asynchronous locking based on a string key
    public sealed class AsyncLock
    {
        private readonly string _key;

        public AsyncLock(string key)
        {
            _key = key;
        }

        private static readonly Dictionary<string, RefCounted<SemaphoreSlim>> SemaphoreSlims = new();

        private static SemaphoreSlim GetOrCreate(string key)
        {
            RefCounted<SemaphoreSlim> item;
            lock (SemaphoreSlims)
            {
                if (SemaphoreSlims.TryGetValue(key, out item))
                {
                    ++item.RefCount;
                }
                else
                {
                    item = new RefCounted<SemaphoreSlim>(new SemaphoreSlim(1, 1));
                    SemaphoreSlims[key] = item;
                }
            }
            return item.Value;
        }

        public static AsyncLock GetLockByKey(string key)
        {
            return new AsyncLock(key);
        }

        public async Task<IDisposable> LockAsync()
        {
            await GetOrCreate(_key).WaitAsync().ConfigureAwait(false);
            return new Releaser(_key);
        }

        public readonly struct Releaser : IDisposable
        {
            private readonly string _key;

            public Releaser(string key)
            {
                _key = key;
            }

            public void Dispose()
            {
                RefCounted<SemaphoreSlim> item;
                lock (SemaphoreSlims)
                {
                    item = SemaphoreSlims[_key];
                    --item.RefCount;
                    if (item.RefCount == 0)
                    {
                        SemaphoreSlims.Remove(_key);
                    }
                }
                item.Value.Release();
            }
        }

        private sealed class RefCounted<T>
        {
            public RefCounted(T value)
            {
                RefCount = 1;
                Value = value;
            }

            public int RefCount { get; set; }
            public T Value { get; private set; }
        }
    }
}
