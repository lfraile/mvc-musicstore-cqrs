using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using MvcMusicStore.App_Start;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof (ClaimsCookieConfig), "PreAppStart")]

namespace MvcMusicStore.App_Start
{
    public static class ClaimsCookieConfig
    {
        public static void PreAppStart()
        {
            DynamicModuleUtility.RegisterModule(typeof (ClaimsCookie.ClaimsCookieModule));
        }
    }
}