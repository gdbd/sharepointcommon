using System;
using Microsoft.SharePoint;

// ReSharper disable once CheckNamespace
namespace SharepointCommon
{
    /// <summary>
    /// Allow to disable or enable firing event receivers in this scope.
    /// Should surrounded in using statement!
    /// </summary>
    public class ListItemEventsScope : SPItemEventReceiver, IDisposable
    {
        private readonly bool _oldValue;

        /// <summary>
        /// Events in scope will be disabled. Avoid to use without using statement!
        /// </summary>
        public ListItemEventsScope()
        {
            _oldValue = EventFiringEnabled;
            EventFiringEnabled = false;
        }

        /// <summary>
        /// Events in scope will be disabled or enabled by value of parameter. Avoid to use without using statement!
        /// </summary>
        /// <param name="isEnabled">true - enabled, false - disabled</param>
        public ListItemEventsScope(bool isEnabled)
        {
            _oldValue = EventFiringEnabled;
            EventFiringEnabled = isEnabled;
        }


        /// <summary>
        /// Reverts EventFiring to original value
        /// </summary>
        public void Dispose()
        {
            EventFiringEnabled = _oldValue;
        }
    }
}
