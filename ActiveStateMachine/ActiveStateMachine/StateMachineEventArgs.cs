using System;

namespace ActiveStateMachine
{
    public class StateMachineEventArgs
    {
        public string EventName { get; set; }
        public string EventInfo { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public StateMachineEventType EventType { get; set; }
    }
}
