using System.IdentityModel.Claims;
using System.Web.Helpers;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.WindowsAzure;
using MvcMusicStore;
using MvcMusicStore.SignalR;
using Owin;

[assembly: OwinStartup(typeof (Startup))]

namespace MvcMusicStore
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            string azureServiceBusSignalR = CloudConfigurationManager.GetSetting("ServiceBusConnectionString");
            GlobalHost.DependencyResolver.UseServiceBus(azureServiceBusSignalR, "Demo.SignalR");
            GlobalHost.DependencyResolver.Register(typeof (IUserIdProvider), () => new SignalRUserIdProvider());
            app.MapSignalR();
            ConfigureAuth(app);

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Name;
        }
    }
}