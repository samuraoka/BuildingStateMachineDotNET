using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ApplicationServices
{
    /// <summary>
    /// This class is responsible to route all events to appropriate
    /// subscribers
    /// </summary>
    public class EventManager
    {
        // Collection of registered events
        private Dictionary<string, object> _eventList;

        // Event manager event is used for logging
        public event EventHandler<StateMachineEventArgs> EventManagerEvent;

        #region singleton implementation
        // Create a thread-safe singleton wit lazy initialization
        private static readonly Lazy<EventManager> _eventManager
            = new Lazy<EventManager>(() => new EventManager());

        public static EventManager Instance
        {
            get
            {
                return _eventManager.Value;
            }
        }

        private EventManager()
        {
            _eventList = new Dictionary<string, object>();
        }
        #endregion

        /// <summary>
        /// Registration of an event used in the system
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="source"></param>
        public void RegisterEvent(string eventName, object source)
        {
            _eventList.Add(eventName, source);
        }

        /// <summary>
        /// Subscription method maps handler method in a sink object to
        /// an event of the source object. Of course, method signatures
        /// between delegate and handler nedd to match!
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handlerMethodName"></param>
        /// <param name="sink"></param>
        /// <returns></returns>
        public bool SubscribeEvent(
            string eventName, string handlerMethodName, object sink)
        {
            try
            {
                // Get event from list
                var evt = _eventList[eventName];

                // Determine meta data from event and handler
                var eventInfo = evt.GetType().GetEvent(eventName);
                var methodInfo = sink.GetType().GetMethod(handlerMethodName);

                // Create new delegate mapping event to handler
                var handler = Delegate.CreateDelegate(
                    eventInfo.EventHandlerType, sink, methodInfo);
                eventInfo.AddEventHandler(evt, handler);

                return true;
            }
            catch (Exception ex)
            {
                // Log failure!
                var message = string.Format("Exception while subscribing to handler. Event: {0} - Handler: {1} - Exception: {2}",
                    eventName, handlerMethodName, ex.ToString());
                Debug.Print(message); //TODO log4net
                RaiseEventManagerEvent("EventManagerSystemEvent",
                    message, StateMachineEventType.System);
                return false;
            }
        }

        private void RaiseEventManagerEvent(string eventName,
            string eventInfo, StateMachineEventType eventType)
        {
            var newArgs = new StateMachineEventArgs(
                eventName, eventInfo, eventType, "Event Manager");
            EventManagerEvent?.Invoke(this, newArgs);
        }
    }
}
