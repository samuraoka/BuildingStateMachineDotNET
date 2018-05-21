using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ConsumerProducer
{
    internal class Consumer
    {
        public Consumer(Queue<int> q, SyncEvents e)
        {
            _queue = q;
            _syncEvents = e;
        }

        public void ThreadRun()
        {
            int count = 0;
            // Remarks - WaitHandle.WaitAny Method
            // https://docs.microsoft.com/en-us/dotnet/api/system.threading.waithandle.waitany?view=netframework-4.7.2#System_Threading_WaitHandle_WaitAny_System_Threading_WaitHandle___
            while (WaitHandle.WaitAny(_syncEvents.EventArray) != 1) //TODO
            {
                lock (((ICollection)_queue).SyncRoot)
                {
                    int item = _queue.Dequeue();
                }
                count += 1;
            }
            Console.WriteLine($"Consumer Thread: consumed {count} items");
        }

        private Queue<int> _queue;
        private SyncEvents _syncEvents;
    }
}
