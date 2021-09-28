using System;
using System.Collections.Generic;

namespace chapterone.services.interfaces
{
    /// <summary>
    /// Interface for a logging service
    /// </summary>
    public interface IEventLogger
    {
        /// <summary>
        /// Log an event
        /// </summary>
        void LogEvent(string eventName, IDictionary<string, string> properties = null);


        /// <summary>
        /// Log an exception
        /// </summary>
        void LogException(Exception exception, IDictionary<string, string> properties = null);
    }
}
