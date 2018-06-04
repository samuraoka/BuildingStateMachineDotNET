using System;

namespace ApplicationServices
{
    /// <summary>
    /// Base class for all devices managed by device manager
    /// </summary>
    public abstract class Device
    {
        Action<string, string, string> _devEventMethod;
        public string DevName { get; private set; }

        public Device(string deviceName,
            Action<string, string, string> eventCallBack)
        {
            DevName = deviceName;
            _devEventMethod = eventCallBack;
        }

        // Device initialization method - need to be implemented in derived
        // classes
        public abstract void OnInit();

        public void RegisterEventCallback(
            Action<string, string, string> method)
        {
            _devEventMethod = method;
        }

        public void DoNotificationCallBack(
            string name, string eventInfo, string source)
        {
            _devEventMethod?.Invoke(name, eventInfo, source);
        }
    }
}
