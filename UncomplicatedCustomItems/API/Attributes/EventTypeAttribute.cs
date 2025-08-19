using System;

namespace UncomplicatedCustomItems.API.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EventTypeAttribute : Attribute
    {
        public Type EventType { get; }
        
        public EventTypeAttribute(Type eventType)
        {
            EventType = eventType;
        }
    }
}