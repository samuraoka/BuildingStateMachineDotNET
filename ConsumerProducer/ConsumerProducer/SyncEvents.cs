using System.Threading;

namespace ConsumerProducer
{
    internal class SyncEvents
    {
        public SyncEvents()
        {
            //TODO
            ExitThreadEvent = new ManualResetEvent(false);
            //TODO
        }

        //TODO
        public EventWaitHandle ExitThreadEvent { get; }
        //TODO
    }
}
