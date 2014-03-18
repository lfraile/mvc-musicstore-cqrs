using MvcMusicStore.CQRS.Core;
using MvcMusicStore.Model;

namespace MvcMusicStore.CQRS.Commands
{
    public class PlaceOrderCommand : ICommand
    {
        public string UserName { get; set; }

        public Order OrderInfo { get; set; }

        public string ShoppingCartId { get; set; }
    }
}