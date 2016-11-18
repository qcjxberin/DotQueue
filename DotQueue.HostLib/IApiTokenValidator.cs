using System;

namespace DotQueue.HostLib
{
    public interface IApiTokenValidator
    {
        bool IsValidToken(string token);
    }

    public interface IExceptionLogger
    {
        void Log(DotQueueException exception);
    }

    public class DotQueueException
    {
        public QueueEventType EventType { get; set; }
        public Exception Exception { get; set; }
        public DateTime ExceptionUtcDateTime { get; set; }
    }

    public enum QueueEventType
    {
        SubscriberOffline,
        MessageIsnull,
    }
}