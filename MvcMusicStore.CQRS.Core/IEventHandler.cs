namespace MvcMusicStore.CQRS.Core
{
    public interface IEventHandler<T> where T : IEvent
    {
        void Handle(T domainEvent);
    }
}