using System.Collections.Generic;
using System.Web.Mvc;
using MongoDB.Driver;
using MvcMusicStore.Infrastructure.Core;
using MvcMusicStore.Model.DTO;
using MvcMusicStore.Model.Extensions;
using MvcMusicStore.ViewModels;

namespace MvcMusicStore.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ICache _cache;
        private readonly MongoDatabase _mongoDatabase;

        public OrdersController(ICache cache, MongoDatabase mongoDatabase)
        {
            _cache = cache;
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
            return _cache.MakeCached(
                User.Identity.Name,
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