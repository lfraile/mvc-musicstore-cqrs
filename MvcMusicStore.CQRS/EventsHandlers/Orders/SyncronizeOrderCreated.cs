using System.Collections.Generic;
using MongoDB.Driver;
using MvcMusicStore.CQRS.Core;
using MvcMusicStore.CQRS.Events;
using MvcMusicStore.Infrastructure;
using MvcMusicStore.Infrastructure.Cache.CacheKeys;
using MvcMusicStore.Model;
using MvcMusicStore.Model.DTO;
using MvcMusicStore.Model.Extensions;

namespace MvcMusicStore.CQRS.EventsHandlers.Orders
{
    public class SyncronizeOrderCreated : IEventHandler<EntityCreatedEvent<Order>>
    {
        private readonly IAzureCache _azureCache;
        private readonly MongoDatabase _mongoDatabase;

        public SyncronizeOrderCreated(IAzureCache azureCache, MongoDatabase mongoDatabase)
        {
            _azureCache = azureCache;
            _mongoDatabase = mongoDatabase;
        }

        public void Handle(EntityCreatedEvent<Order> domainEvent)
        {
            string userName = domainEvent.CreatedByUser;
            Order order = domainEvent.Sender;

            OrderDto orderDto = BuildOrderDto(userName, order);

            MongoCollection<OrderDto> mongoCollection = _mongoDatabase.GetCollection<OrderDto>("LastOrders");
            mongoCollection.Insert(orderDto);

            UpdateCache(mongoCollection, userName);
        }

        private static OrderDto BuildOrderDto(string userName, Order order)
        {
            var orderDto = new OrderDto
            {
                UserName = userName,
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                Items = order.OrderDetails.Count,
                Total = order.Total
            };
            return orderDto;
        }

        private void UpdateCache(MongoCollection<OrderDto> mongoCollection, string userName)
        {
            List<OrderDto> lastOrders = mongoCollection.GetLastOrders(userName);

            _azureCache.Put<List<OrderDto>>(CacheKeys.LastOrders(userName), lastOrders);
        }
    }
}