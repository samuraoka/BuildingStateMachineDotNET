using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ActiveStateMachine
{
    /// <summary>
    /// Base class for active state machines
    /// </summary>
    public class ActiveStateMachine
    {
        public Dictionary<string, State> StateList { get; private set; }
        public BlockingCollection<string> TriggerQueue { get; private set; }
        public State CurrentState { get; private set; }
        public State PreviousState { get; private set; }
        public EngineState StateMachineEngine { get; private set; }
        public event EventHandler<StateMachineEventArgs> StateMachineEvent;

        private Task _queueWorkerTask;
        private readonly State _initialState;
        private ManualResetEvent _resumer;
        private CancellationTokenSource _tokenSource;

        /// <summary>
        /// Constructor for Active State Machine
        /// </summary>
        /// <param name="stateList"></param>
        /// <param name="queueCapacity"></param>
        public ActiveStateMachine(
            Dictionary<string, State> stateList, int queueCapacity)
        {
            // Configure state machine
            StateList = stateList;

            // Anything needs to start somewhere - the initial state
            _initialState = new State("InitialState", null, null, null);

            // Collection taking all triggers. It is thread-safe and blocking
            // as well as FIFO!
            // Limiting its capacity to protect against DoS attack like errors
            // or attacks
            TriggerQueue = new BlockingCollection<string>(queueCapacity);

            // Initialize
            InitStateMachine();

            // Raise an event
            RaiseStateMachineSystemEvent(
                "StateMachine: Initialized", "System ready to start");
            StateMachineEngine = EngineState.Initialized;
        }

        #region state machine engine

        /// <summary>
        /// Start the state machine
        /// </summary>
        public void Start()
        {
            // Create cancellation token for QueueWorker method
            if (_tokenSource != null)
            {
                _tokenSource.Dispose();
                _tokenSource = null;
            }
            _tokenSource = new CancellationTokenSource();

            // Create a new worker thread, if it does not exist
            // Remarks
            // https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskfactory.startnew?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev15.query%3FappId%3DDev15IDEF1%26l%3DEN-US%26k%3Dk(System.Threading.Tasks.TaskFactory.StartNew)%3Bk(DevLang-csharp)%26rd%3Dtrue&view=netframework-4.7.2#remarks
            _queueWorkerTask = Task.Factory.StartNew(QueueWorkerMethod,
                _tokenSource.Token, TaskCreationOptions.LongRunning);

            // Set engine state
            StateMachineEngine = EngineState.Running;
            RaiseStateMachineSystemEvent(
                "StateMachine: Started", "System running.");
        }

        /// <summary>
        /// Pause the state machine worker thread.
        /// </summary>
        public void Pause()
        {
            // Set engine state
            StateMachineEngine = EngineState.Paused;
            _resumer.Reset();
            RaiseStateMachineSystemEvent(
                "StateMachine: Paused", "System waiting.");
        }

        /// <summary>
        /// Resume the state machine worker thread.
        /// </summary>
        public void Resume()
        {
            // Worker thread exists, just resume where it was paused.
            _resumer.Set();
            // Set engine state
            StateMachineEngine = EngineState.Running;
            RaiseStateMachineSystemEvent(
                "StateMachine: Resumed", "System running.");
        }

        /// <summary>
        /// Ends queue processing
        /// </summary>
        public void Stop()
        {
            // Cancel processing
            _tokenSource.Cancel();
            // Wait for thread to return
            _queueWorkerTask.Wait();
            // Free resources
            _queueWorkerTask.Dispose();
            _queueWorkerTask = null;
            // Set engine state
            StateMachineEngine = EngineState.Stopped;
            RaiseStateMachineSystemEvent(
                "StateMachine: Stopped", "System execution stopped.");
        }

        /// <summary>
        /// Initialize state machine, but does not start it
        /// -> dedicated start method
        /// </summary>
        private void InitStateMachine()
        {
            // Set previous state to an unspecific initial state. The initial
            // state never will be used during normal operation
            PreviousState = _initialState;

            // Look for the default state, which is the state to begin with
            // in the StateList
            CurrentState = StateList
                .Where(state => state.Value.IsDefaultState)
                .Select(state => state.Value).First();
            RaiseStateMachineSystemCommand(
                "OnInit", "StateMachineInitialized");

            // This is the synchronization object for resuming - passing true
            // means non-blocking (signaled), which is the normal operation 
            // mode
            _resumer = new ManualResetEvent(true);
        }

        /// <summary>
        /// Enter a trigger to the queue
        /// </summary>
        /// <param name="newTrigger"></param>
        private void EnterTrigger(string newTrigger)
        {
            // Put trigger in queue
            try
            {
                TriggerQueue.Add(newTrigger);
            }
            catch (Exception ex)
            {
                RaiseStateMachineSystemEvent(
                    "ActiveStateMachine - Error entering trigger",
                    $"{newTrigger} -  {ex.ToString()}");
            }
            // Raise an event
            RaiseStateMachineSystemEvent(
                "ActiveStateMachine - Trigger entered", newTrigger);
        }

        /// <summary>
        /// Worker method for trigger queue
        /// </summary>
        /// <param name="token">a instatnce of CancellationToken</param>
        private void QueueWorkerMethod(object token)
        {
            var cancellationToken = (CancellationToken)token;

            // Blocks execution until it is reset.
            // Used to pause the state machine.
            _resumer.WaitOne();

            // Block the queue and loop through all triggers available.
            // Blocking queue guarantees FIFO and the GetConsumingEnumerable
            // method automatically removes triggers from queue!
            try
            {
                foreach (var trigger in TriggerQueue.GetConsumingEnumerable())
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        RaiseStateMachineSystemEvent(
                            "State machine: QueueWorker",
                            "Processing canceled!");
                        return;
                    }

                    // Compare trigger
                    foreach (var transition in
                        CurrentState.StateTransitionList
                            .Where(t => trigger == t.Value.Trigger))
                    {
                        ExecuteTransition(transition.Value);
                    }
                }

                // Do not place any code here, because it will not be
                // executed! The foreach loop keeps spinning on the queue
                // until thread is canceled.
            }
            catch (Exception ex)
            {
                RaiseStateMachineSystemEvent(
                    "State machine: QueueWorker",
                    "Processing canceled! Exception: " + ex.ToString());
                // Create a new queue worker task. The previous one is
                // completing right now.
                Start();
            }
        }

        /// <summary>
        /// Transition to a new state
        /// </summary>
        /// <param name="transition"></param>
        protected virtual void ExecuteTransition(Transition transition)
        {
            // Default checking, if this is a valid transition.
            if (CurrentState.StateName != transition.SourceStateName)
            {
                var message = string.Format(
                    "Transition has wrong source state {0}, when system is in {1}",
                    transition.SourceStateName, CurrentState.StateName);
                RaiseStateMachineSystemEvent(
                    "State machine: Default guard execute transition.",
                    message);
                return;
            }
            if (StateList.ContainsKey(transition.TargetStateName) == false)
            {
                var message = string.Format(
                    "Transition has wrong target state {0}, when system is in {1}. State not in global state list",
                    transition.SourceStateName, CurrentState.StateName);
                RaiseStateMachineSystemEvent(
                    "State machine: Default guard execute transition.",
                    message);
                return;
            }

            // Run all exit actions of the old state
            CurrentState.ExitActions.ForEach(a => a.Execute());

            // Run all guards of the transition
            transition.GuardList.ForEach(g => g.Execute());
            string info =
                transition.GuardList.Count + " guard actions executed!";
            RaiseStateMachineSystemEvent(
                "State machine: ExecuteTransition", info);

            // Run all actions of the transition
            transition.TransitionActionList.ForEach(a => a.Execute());

            //////////////////
            // Stage change
            //////////////////
            info = transition.TransitionActionList.Count +
                " transition actions executed!";
            RaiseStateMachineSystemEvent(
                "State machine: Begin state change!", info);

            // First resolve the target state with the help of its name
            var targetState =
                GetStateFromStateList(transition.TargetStateName);

            // Transition successful - Change state
            PreviousState = CurrentState;
            CurrentState = targetState;

            // Run all entry actions of new state
            CurrentState.EntryActions.ForEach(a => a.Execute());
            RaiseStateMachineSystemEvent(
                "State machine: State change completed successfully!",
                $"Previous state: {PreviousState.StateName} - New state = {CurrentState.StateName}");
        }

        /// <summary>
        /// Helper to load state from state list
        /// </summary>
        /// <param name="targetStateName"></param>
        /// <returns></returns>
        private State GetStateFromStateList(string targetStateName)
        {
            return StateList[targetStateName];
        }
        #endregion

        #region event infrastructure
        /// <summary>
        /// Helper method to raise state machine system events
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventInfo"></param>
        private void RaiseStateMachineSystemEvent(
            string eventName, string eventInfo)
        {
            // Raise event only, if subscriber exist.
            // Otherwise an exception occurs.
            StateMachineEvent?.Invoke(this, new StateMachineEventArgs(
                eventName, eventInfo, StateMachineEventType.System,
                "State machine"));
        }

        /// <summary>
        /// Raise an event of type command
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventInfo"></param>
        private void RaiseStateMachineSystemCommand(
            string eventName, string eventInfo)
        {
            // Raise event only, if subscriber exist.
            // Otherwise an exception occurs
            StateMachineEvent?.Invoke(this, new StateMachineEventArgs(
                eventName, eventInfo, StateMachineEventType.Command,
                "State machine"));
        }

        public void InternalNotificationHandler(
            object sender, StateMachineEventArgs args)
        {
            EnterTrigger(args.EventName);
        }
        #endregion
    }
}
