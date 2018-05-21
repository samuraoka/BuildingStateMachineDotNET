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
            //TODO
        }

        private Queue<int> _queue;
        private SyncEvents _syncEvents;
    }
}
