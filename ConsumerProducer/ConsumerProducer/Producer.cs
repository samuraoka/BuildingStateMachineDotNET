using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ConsumerProducer
{
    internal class Producer
    {
        public Producer(BlockingCollection<int> q, CancellationToken t)
        {
            _queue = q;
            _cancel = t;
        }

        public void ThreadRun()
        {
            int count = 0;
            var r = new Random(Guid.NewGuid().GetHashCode());

            while (_cancel.IsCancellationRequested == false)
            {
                while (_queue.Count < 20)
                {
                    _queue.Add(r.Next(0, 100));
                    count += 1;
                }
            }
            Console.WriteLine($"Producer thread: produced {count} items");
        }

        private BlockingCollection<int> _queue;
        private CancellationToken _cancel;
    }
}
