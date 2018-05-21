using System.Threading;

namespace ConsumerProducer
{
    internal class SyncEvents
    {
        public SyncEvents()
        {
            NewItemEvent = new AutoResetEvent(false);
            ExitThreadEvent = new ManualResetEvent(false);
            EventArray = new WaitHandle[]
            {
                NewItemEvent,
                ExitThreadEvent,
            };
        }

        public EventWaitHandle ExitThreadEvent { get; }
        public EventWaitHandle NewItemEvent { get; }
        public WaitHandle[] EventArray { get; }
    }
}
