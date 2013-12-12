using System;
using System.Web.Mvc;
using MvcMusicStore.CQRS.Commands;
using MvcMusicStore.Infrastructure;
using MvcMusicStore.Model;

namespace MvcMusicStore.Controllers
{
    public class CheckoutController : Controller
    {
        private const string PromoCode = "FREE";
        private readonly IMessageBus _bus;
        private readonly ICartStoreService _cartStoreService;
        private readonly MusicStoreEntities _storeDb = new MusicStoreEntities();

        public CheckoutController(IMessageBus bus, ICartStoreService cartStoreService)
        {
            _bus = bus;
            _cartStoreService = cartStoreService;
        }

        //
        // GET: /Checkout/AddressAndPayment

        public ActionResult AddressAndPayment()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        //
        // POST: /Checkout/AddressAndPayment

        [HttpPost]
        public ActionResult AddressAndPayment(FormCollection values)
        {
            var order = new Order();
            TryUpdateModel(order);

            try
            {
                if (string.Equals(values["PromoCode"], PromoCode,
                    StringComparison.OrdinalIgnoreCase) == false)
                {
                    return View(order);
                }

                _bus.Send(
                    new AddressAndPaymentCommand
                    {
                        UserName = User.Identity.Name,
                        OrderInfo = order
                    });

                return RedirectToAction("Complete");
            }
            catch
            {
                //Invalid - redisplay with errors
                return View(order);
            }
        }

        //
        // GET: /Checkout/Complete

        public ActionResult Complete()
        {
            return View();
        }
    }
}