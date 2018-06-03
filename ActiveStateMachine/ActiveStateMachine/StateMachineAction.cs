using System;

/*
=========1=========2=========3=========4=========5=========6=========7=========8
*/
namespace ActiveStateMachine
{
    /// <summary>
    /// Class for any actions in the state machine. It is used for guards,
    /// transition actions as well as entry / exit actions
    /// </summary>
    public class StateMachineAction
    {
        /// <summary>
        /// The name of this StateMachineAction
        /// </summary>
        public string Name { get; private set; }

        /// Delegate pointing to the implementation of method
        /// to be executed
        private Action _method;

        /// <summary>
        /// Constructor for state machine action
        /// </summary>
        /// <param name="name"></param>
        /// <param name="method"></param>
        public StateMachineAction(string name, Action method)
        {
            Name = name;
            _method = method;
        }

        /// <summary>
        /// Method running the action.
        /// Will be called e.g. by state machine, when a transition is
        /// executed. Could also be used in a guard, entry or exit action.
        /// </summary>
        public void Execute()
        {
            _method.Invoke();
        }
    }
}
/*
=========1=========2=========3=========4=========5=========6=========7=========8
*/
