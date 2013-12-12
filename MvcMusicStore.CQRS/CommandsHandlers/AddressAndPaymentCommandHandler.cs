using System;
using MvcMusicStore.CQRS.Commands;
using MvcMusicStore.CQRS.Core;
using MvcMusicStore.CQRS.Events;
using MvcMusicStore.Infrastructure;
using MvcMusicStore.Model;

namespace MvcMusicStore.CQRS.CommandsHandlers
{
    public class AddressAndPaymentCommandHandler : ICommandHandler<AddressAndPaymentCommand>
    {
        private readonly ICartStoreService _cartStoreService;
        private readonly IMessageBus _messageBus;

        public AddressAndPaymentCommandHandler(
            IMessageBus messageBus,
            ICartStoreService cartStoreService)
        {
            _messageBus = messageBus;
            _cartStoreService = cartStoreService;
        }

        public void Execute(AddressAndPaymentCommand command)
        {
            Order order = command.OrderInfo;
            order.Username = command.UserName;
            order.OrderDate = DateTime.Now;

            ShoppingCart cart = ShoppingCart.GetCart(_cartStoreService);
            cart.CreateOrder(order);

            _messageBus.Publish(new EntityCreatedEvent<Order>(order.Username, command.OrderInfo));
        }
    }
}