using Common;
using System;
using System.Linq;

namespace ApplicationServices
{
    public class ViewManager
    {
        private string[] _viewStates;
        private string _defaultViewState;
        // UI - make this a Dictioanry<string, IUserInterface>, if you have to
        // handle more than one
        private IUserInterface _UI;

        public event EventHandler<StateMachineEventArgs> ViewManagerEvent;
        public string CurrentView { get; private set; }
        public IViewStateConfiguration ViewStateConfiguration { get; set; }

        #region singleton implementation
        private static Lazy<ViewManager> _viewManager
            = new Lazy<ViewManager>(() => new ViewManager());

        public ViewManager Instance
        {
            get
            {
                return _viewManager.Value;
            }
        }

        private ViewManager()
        {
        }
        #endregion

        public void LoadViewStateConfiguration(
            IViewStateConfiguration viewStateConfiguration,
            IUserInterface userInterface)
        {
            ViewStateConfiguration = ViewStateConfiguration;
            _viewStates = ViewStateConfiguration.ViewStateList;
            _UI = userInterface;
            _defaultViewState = ViewStateConfiguration.DefaultViewState;
        }

        /// <summary>
        /// Handler method for state machine commands
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ViewCommandHandler(
            object sender, StateMachineEventArgs args)
        {
            // This approach assumes that there is a dedicated view state
            // for every state machine UI command.
            try
            {
                if (_viewStates.Contains(args.EventName))
                {
                    // Convention: view command event names matches
                    // corresponding view state
                    _UI.LoadViewState(args.EventName);
                    CurrentView = args.EventName;
                    RaiseViewManagerEvent("View Manager Command",
                        $"Successfully loaded view state: {args.EventName}");
                }
                else
                {
                    RaiseViewManagerEvent("View Manager Command",
                        $"View state not found: {args.EventName}");
                }
            }
            catch (Exception ex)
            {
                RaiseViewManagerEvent(
                    "View Manager Command - Error", ex.ToString());
            }
        }

        public void SystemEventHandler(
            object sender, StateMachineEventArgs args)
        {
            // Initialize
            if (args.EventName == "OnInit")
            {
                _UI.LoadViewState(_defaultViewState);
                CurrentView = _defaultViewState;
            }
        }

        /// <summary>
        /// Method to raise a view manager event for logging, etc
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        /// <param name="eventType"></param>
        private void RaiseViewManagerEvent(string name, string info,
            StateMachineEventType eventType = StateMachineEventType.System)
        {
            var newVMargs = new StateMachineEventArgs(name,
                $"View manager event: {info}", eventType, "View Manager");

            // Raise event only, if there are subscribers!
            ViewManagerEvent?.Invoke(this, newVMargs);
        }

        /// <summary>
        /// Sends a command to another service
        /// </summary>
        /// <param name="command"></param>
        /// <param name="info"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public void RaiseUICommand(
            string command, string info, string source, string target)
        {
            var newUIargs = new StateMachineEventArgs(
                command, info, StateMachineEventType.Command, source, target);
            ViewManagerEvent?.Invoke(this, newUIargs);
        }
    }
}
