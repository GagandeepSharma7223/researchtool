using chapterone.shared;
using chapterone.services.interfaces;

using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Threading;

namespace chapterone.web.logging
{
    public class AppInsightsEventLogger : IEventLogger
    {
        private const int TIMER_DELAY = 0;

        private readonly TelemetryClient _client = new TelemetryClient();
        private readonly Timer _heartbeat;

        /// <summary>
        /// Constructor
        /// </summary>
        public AppInsightsEventLogger(string instrumentationKey)
        {
            _client.InstrumentationKey = instrumentationKey;
            _heartbeat = new Timer(HeartbeatCallback, this, TIMER_DELAY, EventConstants.HEARTBEAT_INTERVAL_MS);
        }


        /// <summary>
        /// Log an event in AppInsights
        /// </summary>
        public void LogEvent(string eventName, IDictionary<string, string> properties = null)
        {
            _client.TrackEvent(eventName, properties);
        }


        /// <summary>
        /// Log an exception in AppInsights
        /// </summary>
        /// <param name="exception"></param>
        public void LogException(Exception exception, IDictionary<string, string> properties = null)
        {
            _client.TrackException(exception, properties);
        }


        #region Private methods
        
        /// <summary>
        /// Fires every heartbeat interval
        /// </summary>
        private void HeartbeatCallback(object state)
        {
            var self = state as AppInsightsEventLogger;

            self._client.TrackEvent("Heartbeat");
        }

        #endregion
    }
}
