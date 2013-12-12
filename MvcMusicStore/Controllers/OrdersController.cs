using System.Collections.Generic;
using System.Web.Mvc;
using MongoDB.Driver;
using MvcMusicStore.Infrastructure;
using MvcMusicStore.Infrastructure.Cache.CacheKeys;
using MvcMusicStore.Model.DTO;
using MvcMusicStore.Model.Extensions;
using MvcMusicStore.ViewModels;

namespace MvcMusicStore.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IAzureCache _azureCache;
        private readonly MongoDatabase _mongoDatabase;

        public OrdersController(IAzureCache azureCache, MongoDatabase mongoDatabase)
        {
            _azureCache = azureCache;
            _mongoDatabase = mongoDatabase;
        }

        //
        // GET: /Orders/
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            List<OrderDto> lastOrdersCached = GetLastOrdersCached();

            var ordersViewModel = new OrdersViewModel
            {
                LastOrders = lastOrdersCached
            };

            return View(ordersViewModel);
        }


        private List<OrderDto> GetLastOrdersCached()
        {
            return _azureCache.MakeCached(
                CacheKeys.LastOrders(User.Identity.Name),
                key => GetLastOrderFromDb());
        }

        private List<OrderDto> GetLastOrderFromDb()
        {
            var mongoCollection = _mongoDatabase.GetCollection<OrderDto>("LastOrders");

            return mongoCollection.GetLastOrders(User.Identity.Name);
        }

        public ActionResult Details()
        {
            return View();
        }
    }
}