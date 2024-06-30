using Shield.Common.Domain;

namespace Shield.Common
{
    public static class Util
    {
        /// <summary>
        /// Gets or create <see cref="Mutex"/> to avoid concurrency between <see cref="Services.SharedMemoryService"/> instances.
        /// </summary>
        public static Mutex StartMutex()
        {
            lock (new object())
            {
                Mutex mutex;

                try
                {
                    mutex = Mutex.OpenExisting(Constants.SERVICE_MUTEX);
                }
                catch (WaitHandleCannotBeOpenedException)
                {
                    mutex = new Mutex(false, Constants.SERVICE_MUTEX);
                }

                mutex.WaitOne();

                return mutex;
            }
        }
    }
}
