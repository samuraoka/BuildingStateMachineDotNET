using System.Collections.Generic;

namespace Common
{
    /// <summary>
    /// Interface for a viewstate configuration
    /// </summary>
    public interface IViewStateConfiguration
    {
        Dictionary<string, object> ViewStates { get; set; }
        string[] ViewStateList { get; set; }
        string DefaultViewState { get; set; }
    }
}
