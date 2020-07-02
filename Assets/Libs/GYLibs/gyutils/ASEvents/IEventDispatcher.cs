namespace GYLib.Utils.ASEvents
{
    public interface IEventDispatcher
    {
        void AddEventListener(string eventType, EventListener listener, int priority = 0);
        void DispatchEvent(ASEvent e);
        bool HasEventListener(string eventType);
        void RemoveEventListener(string eventType, EventListener listener);
    }
}