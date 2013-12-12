using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using MvcMusicStore.CQRS.Core;
using Ninject;

namespace MvcMusicStore.Infrastructure.Azure
{
    public class MessageBus : IMessageBus
    {
        private readonly QueueClient _client;
        private readonly string _connectionString;
        private readonly IKernel _kernel;
        private readonly string _path;

        public MessageBus(IKernel kernel, string connectionString, string path)
        {
            _kernel = kernel;
            _connectionString = connectionString;
            _path = path;

            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.QueueExists(path))
            {
                namespaceManager.CreateQueue(path);
            }

            _client = QueueClient.CreateFromConnectionString(connectionString, path);
        }

        public void Send<T>(T command) where T : ICommand
        {
            var commandHandler = _kernel.Get<ICommandHandler<T>>();

            if (commandHandler == null)
            {
                throw new CommandHandlerNotFoundException(typeof (T));
            }

            commandHandler.Execute(command);
        }

        public Task SendAsync<T>(T command) where T : ICommand
        {
            throw new NotImplementedException();
        }

        public void Publish<T>(T @event) where T : IEvent
        {
            var brokeredMesage = new BrokeredMessage(@event);
            brokeredMesage.Properties["MessageType"] = @event.GetType().AssemblyQualifiedName;
            _client.Send(brokeredMesage);
        }

        public Task PublishAsync<T>(T @event) where T : IEvent
        {
            throw new NotImplementedException();
        }
    }

    public class CommandHandlerNotFoundException : Exception
    {
        private readonly Type _type;

        public CommandHandlerNotFoundException(Type type)
        {
            _type = type;
        }
    }
}