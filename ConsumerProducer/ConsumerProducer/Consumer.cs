using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ConsumerProducer
{
    internal class Consumer
    {
        public Consumer(BlockingCollection<int> q, CancellationToken t)
        {
            _queue = q;
            _cancel = t;
        }

        public void ThreadRun()
        {
            int count = 0;
            while (_cancel.IsCancellationRequested == false)
            {
                int item = _queue.Take();
                count += 1;
            }
            Console.WriteLine($"Consumer Thread: consumed {count} items");
        }

        private BlockingCollection<int> _queue;
        private CancellationToken _cancel;
    }
}
