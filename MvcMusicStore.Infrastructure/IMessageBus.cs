using System.Threading.Tasks;
using MvcMusicStore.CQRS.Core;

namespace MvcMusicStore.Infrastructure
{
    public interface IMessageBus
    {
        void Send<T>(T command) where T : ICommand;

        Task SendAsync<T>(T command) where T : ICommand;

        void Publish<T>(T @event) where T : IEvent;

        Task PublishAsync<T>(T @event) where T : IEvent;
    }
}