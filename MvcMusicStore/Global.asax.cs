using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Beezy.Infraestructure.Cache;
using Microsoft.WindowsAzure;
using MongoDB.Driver;
using MvcMusicStore.App_Start;
using MvcMusicStore.CQRS.CommandsHandlers;
using MvcMusicStore.Infrastructure;
using MvcMusicStore.Infrastructure.Azure;
using MvcMusicStore.Model;
using Ninject;
using Ninject.Extensions.Conventions;

namespace MvcMusicStore
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer<MusicStoreEntities>(null);

            var kernel = BuildNinjectKernel();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DependencyResolver.SetResolver(new NinjectMvcDependencyResolver(kernel));
        }

        private IKernel BuildNinjectKernel()
        {
            var connectionString = CloudConfigurationManager.GetSetting("MongodbConnectionstring");
            var database = CloudConfigurationManager.GetSetting("MongodbDatabase");
            var mongoClient = new MongoClient(connectionString);
            var mongoServer = mongoClient.GetServer();
            var mongoDatabase = mongoServer.GetDatabase(database);

            var kernel = new StandardKernel();

            kernel.Bind<MongoDatabase>()
                .ToConstant(mongoDatabase);

            kernel.Bind<ICartStoreService>()
                .To<SessionCartStoreService>();

            kernel.Bind(
                x => x.FromAssemblyContaining(typeof (AddressAndPaymentCommandHandler))
                    .SelectAllClasses()
                    .BindAllInterfaces());

            kernel.Bind<IAzureCache>()
                .To<AzureCache>();

            kernel.Bind<IMessageBus>()
                .To<MessageBus>()
                .InSingletonScope()
                .WithConstructorArgument("kernel", kernel)
                .WithConstructorArgument("connectionString",
                    CloudConfigurationManager.GetSetting("ServiceBusConnectionString"))
                .WithConstructorArgument("path", "CQRSTEST-QUEUE");

            return kernel;
        }
    }
}