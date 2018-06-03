using System.Collections.Generic;

namespace ActiveStateMachine
{
    /// <summary>
    /// Base class for a state
    /// implementations derive from it
    /// </summary>
    public class State
    {
        public string StateName { get; private set; }
        public Dictionary<string, Transition> StateTransitionList
        { get; private set; }
        public List<StateMachineAction> EntryActions { get; private set; }
        public List<StateMachineAction> ExitActions { get; private set; }
        public bool IsDefaultState { get; private set; }

        /// <summary>
        /// Constructor for a state
        /// </summary>
        /// <param name="name"></param>
        /// <param name="transitionList"></param>
        /// <param name="entryActions"></param>
        /// <param name="exitActions"></param>
        /// <param name="defaultState"></param>
        public State(string name,
            Dictionary<string, Transition> transitionList,
            List<StateMachineAction> entryActions,
            List<StateMachineAction> exitActions, bool defaultState = false)
        {
            StateName = name;
            StateTransitionList = transitionList;
            IsDefaultState = defaultState;
            EntryActions = entryActions;
            ExitActions = exitActions;
        }
    }
}
