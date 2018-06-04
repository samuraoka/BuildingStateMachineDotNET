using Common;
using System;
using System.Diagnostics;

namespace ApplicationServices
{
    /// <summary>
    /// Simple logging class for state machine and service events
    /// </summary>
    public class LogManager
    {
        #region singleton implementation
        // Create a thread-safe singleton wit lazy initialization
        private static readonly Lazy<LogManager> _logger
            = new Lazy<LogManager>(() => new LogManager());

        public static LogManager Instance
        {
            get
            {
                return _logger.Value;
            }
        }

        private LogManager()
        {

        }
        #endregion

        /// <summary>
        /// Log infos to debug window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void LogEventHandler(object sender, StateMachineEventArgs args)
        {
            if (args.EventType != StateMachineEventType.Notification)
            {
                // Log system events
                //TODO log4net
                Debug.Print("{0} SystemEvent: {1} - Info: {2} - StateMachineArgumentType: {3} - Source: {4} - Target: {5}",
                    args.TimeStamp, args.EventName, args.EventInfo,
                    args.EventType, args.Source, args.Target);
            }
            else
            {
                // Log state machine notifications
                //TODO log4net
                Debug.Print("{0} Notification: {1} - Info: {2} - StateMachineArgumentType: {3} - Source: {4} - Target: {5}",
                    args.TimeStamp, args.EventName, args.EventInfo,
                    args.EventType, args.Source, args.Target);
            }
        }
    }
}
