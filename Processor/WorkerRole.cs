using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;
using Beezy.Infraestructure.Cache;
using Microsoft.AspNet.SignalR;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using MongoDB.Driver;
using MvcMusicStore.CQRS.CommandsHandlers;
using MvcMusicStore.CQRS.Core;
using MvcMusicStore.Infrastructure;
using MvcMusicStore.Model;
using Ninject;
using Ninject.Extensions.Conventions;
using ReflectionMagic;

namespace Processor
{
    public class WorkerRole : RoleEntryPoint
    {
        private StandardKernel _kernel;
        private QueueClient _subscriptionClient;

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("Processor entry point called", "Information");

            while (true)
            {
                var brokeredMessage = _subscriptionClient.Receive();
                if (brokeredMessage != null)
                {
                    var typeName =
                        brokeredMessage.Properties["MessageType"].ToString();
                    Type typeArguments = Type.GetType(typeName);
                    Type type = typeof (IEventHandler<>).MakeGenericType(typeArguments);
                    var eventHandlers = _kernel.GetAll(type);

                    MethodInfo method = typeof (BrokeredMessage).GetMethod("GetBody", new Type[] {});
                    MethodInfo generic = method.MakeGenericMethod(typeArguments);
                    dynamic messageBody = generic.Invoke(brokeredMessage, null);

                    foreach (var handler in eventHandlers)
                    {
                        handler.AsDynamic().Handle(messageBody);
                    }

                    brokeredMessage.Complete();
                }

                Thread.Sleep(10000);
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            ConfigureIoC();
            ConfigureServiceBus();
            ConfigureSignalR();

            var azureCache = _kernel.Get<IAzureCache>();

            return base.OnStart();
        }

        private void ConfigureSignalR()
        {
            var azureServiceBusSignalR = CloudConfigurationManager.GetSetting("ServiceBusConnectionString");
            GlobalHost.DependencyResolver.UseServiceBus(azureServiceBusSignalR, "Demo.SignalR");
        }

        private void ConfigureIoC()
        {
            var connectionString = CloudConfigurationManager.GetSetting("MongodbConnectionstring");
            var database = CloudConfigurationManager.GetSetting("MongodbDatabase");
            var mongoClient = new MongoClient(connectionString);
            var mongoServer = mongoClient.GetServer();
            var mongoDatabase = mongoServer.GetDatabase(database);

            _kernel = new StandardKernel();

            _kernel.Bind<IAzureCache>().
                To<AzureCache>();

            _kernel.Bind<MusicStoreEntities>().ToSelf();
            _kernel.Bind<MongoDatabase>().ToConstant(mongoDatabase);

            _kernel.Bind(
                x => x.FromAssemblyContaining(typeof (AddressAndPaymentCommandHandler))
                    .SelectAllClasses()
                    .BindAllInterfaces()
                    .Configure(c => c.InSingletonScope()));
        }

        private void ConfigureServiceBus()
        {
            var connectionString = CloudConfigurationManager.GetSetting("ServiceBusConnectionString");
            const string topicPath = "CQRSTEST-QUEUE";

            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.QueueExists(topicPath))
            {
                try
                {
                    namespaceManager.CreateQueue(topicPath);
                }
                catch (MessagingEntityAlreadyExistsException)
                {

                }
            }

            _subscriptionClient = QueueClient.CreateFromConnectionString(connectionString, topicPath);
        }
    }
}