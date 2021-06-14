using System.Collections.Generic;
using System.Threading;

namespace BugsnagUnity
{
    class BlockingQueue<T>
    {
        Queue<T> Queue { get; }
        object QueueLock { get; }

        internal BlockingQueue()
        {
            QueueLock = new object();
            Queue = new Queue<T>();
        }

        internal void Enqueue(T item)
        {
            lock (QueueLock)
            {
                Queue.Enqueue(item);
                Monitor.Pulse(QueueLock);
            }
        }

        internal T Dequeue()
        {
            lock (QueueLock)
            {
                while (Queue.Count == 0)
                {
                    Monitor.Wait(QueueLock);
                }

                return Queue.Dequeue();
            }
        }
    }
}
