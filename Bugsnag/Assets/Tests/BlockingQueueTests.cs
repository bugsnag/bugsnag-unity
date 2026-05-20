using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using BugsnagUnity;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class BlockingQueueTests
    {
        [Test]
        public void Enqueue_ThenDequeue_ReturnsSameItem()
        {
            var queue = new BlockingQueue<string>();
            queue.Enqueue("hello");
            Assert.AreEqual("hello", queue.Dequeue());
        }

        [Test]
        public void Enqueue_MultipleItems_DequeuesInFIFOOrder()
        {
            var queue = new BlockingQueue<int>();
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);

            Assert.AreEqual(1, queue.Dequeue());
            Assert.AreEqual(2, queue.Dequeue());
            Assert.AreEqual(3, queue.Dequeue());
        }

        [Test]
        public void Dequeue_BlocksUntilItemEnqueued()
        {
            var queue = new BlockingQueue<string>();
            string result = null;

            var thread = new Thread(() =>
            {
                result = queue.Dequeue();
            });
            thread.Start();

            Thread.Sleep(50); // give dequeue time to block
            queue.Enqueue("unblocked");
            thread.Join(1000);

            Assert.AreEqual("unblocked", result);
        }

        [Test]
        public void Enqueue_FromMultipleThreads_AllItemsDequeued()
        {
            var queue = new BlockingQueue<int>();
            int threadCount = 5;

            for (int i = 0; i < threadCount; i++)
            {
                int value = i;
                new Thread(() => queue.Enqueue(value)).Start();
            }

            var dequeued = new List<int>();
            for (int i = 0; i < threadCount; i++)
            {
                dequeued.Add(queue.Dequeue());
            }

            Assert.AreEqual(threadCount, dequeued.Count);
        }
    }
}
