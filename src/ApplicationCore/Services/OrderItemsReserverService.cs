using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.Extensions.Configuration;
using Microsoft.eShopWeb.ApplicationCore.Entities.AzureStorage;
using Azure.Messaging.ServiceBus;

namespace Microsoft.eShopWeb.ApplicationCore.Services;
public class OrderItemsReserverService: IAzureStorageService
{
    private readonly IRepository<Basket> _basketRepository;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public OrderItemsReserverService(HttpClient httpClient, 
        IRepository<Basket> basketRepository,
        IConfiguration configuration)
    {
        _basketRepository = basketRepository;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task AddOrder(int basketId)
    {
        var basketSpec = new BasketWithItemsSpecification(basketId);
        var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);

        Guard.Against.Null(basket, nameof(basket));
        Guard.Against.EmptyBasketOnCheckout(basket.Items);
       
        var orderedItems = basket.Items
            .Select(item => new OrderItem
            {
                CatalogItemId = item.Id,
                Quantity = item.Quantity
            }).ToArray();

        var storageOrder = new Order()
        {
            BuyerId = basket.BuyerId,
            OrderedItems = orderedItems
        };


        string serviceBusConnectionString = _configuration["OrderItemsReserver:ServiceBusConnectionString"] ?? "";
        string queueName = _configuration["OrderItemsReserver:QueueName"] ?? "";

        ServiceBusClient client = new ServiceBusClient(serviceBusConnectionString);
        ServiceBusSender sender = client.CreateSender(queueName);
        var jsonMessage = JsonSerializer.Serialize(storageOrder);
        ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage));
        await sender.SendMessageAsync(message);
    }
}
