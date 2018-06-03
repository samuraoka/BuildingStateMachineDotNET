using System.Collections.Generic;

namespace ActiveStateMachine
{
    /// <summary>
    /// State machine transition class
    /// </summary>
    public class Transition
    {
        public string Name { get; private set; }
        public string SourceStateName { get; private set; }
        public string TargetStateName { get; private set; }
        public List<StateMachineAction> GuardList { get; private set; }
        public List<StateMachineAction> TransitionActionList
        { get; private set; }
        public string Trigger { get; private set; }

        /// <summary>
        /// Constructor for Transition
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sourceState"></param>
        /// <param name="targetState"></param>
        /// <param name="guardList"></param>
        /// <param name="transitionActionList"></param>
        /// <param name="grigger"></param>
        public Transition(string name, string sourceState, string targetState,
            List<StateMachineAction> guardList,
            List<StateMachineAction> transitionActionList, string grigger)
        {
            Name = name;
            SourceStateName = sourceState;
            TargetStateName = targetState;
            GuardList = guardList;
            TransitionActionList = transitionActionList;
            Trigger = Trigger;
        }
    }
}
