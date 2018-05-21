using System;
using System.Collections;
using System.Collections.Generic;

namespace ConsumerProducer
{
    internal class Producer
    {
        public Producer(Queue<int> q, SyncEvents e)
        {
            _queue = q;
            _syncEvents = e;
        }

        public void ThreadRun()
        {
            int count = 0;
            var r = new Random(Guid.NewGuid().GetHashCode());

            while (_syncEvents.ExitThreadEvent.WaitOne(0, false) == false) //TODO
            {
                lock (((ICollection)_queue).SyncRoot)
                {
                    while (_queue.Count < 20)
                    {
                        _queue.Enqueue(r.Next(0, 100));
                        _syncEvents.NewItemEvent.Set();
                        count += 1;
                    }
                }
            }
            Console.WriteLine($"Producer thread: produced {count} items");
        }

        private Queue<int> _queue;
        private SyncEvents _syncEvents;
    }
}
