using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.StorageEntities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.Extensions.Configuration;

namespace Microsoft.eShopWeb.ApplicationCore.Services;
public class StorageService: IStorageService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IUriComposer _uriComposer;
    private readonly IRepository<Basket> _basketRepository;
    private readonly IRepository<CatalogItem> _itemRepository;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public StorageService(HttpClient httpClient, 
        IRepository<Basket> basketRepository,
        IRepository<CatalogItem> itemRepository,
        IRepository<Order> orderRepository,
        IUriComposer uriComposer,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _orderRepository = orderRepository;
        _uriComposer = uriComposer;
        _basketRepository = basketRepository;
        _itemRepository = itemRepository;
        _configuration = configuration;
    }

    public async Task AddToOrderItemsReserverStorage(int basketId)
    {
        var basketSpec = new BasketWithItemsSpecification(basketId);
        var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);

        Guard.Against.Null(basket, nameof(basket));
        Guard.Against.EmptyBasketOnCheckout(basket.Items);
       
        var orderedItems = basket.Items
            .Select(item => new StorageOrderItemInfo
            {
                CatalogItemId = item.Id,
                Quantity = item.Quantity
            }).ToArray();

        var storageOrder = new StorageOrder()
        {
            BuyerId = basket.BuyerId,
            OrderedItems = orderedItems
        };

        string storageUrl = _configuration["OrderItemsReserverUrl"] ?? "";
        string storageKey = _configuration["OrderItemsReserverKey"] ?? "";

        if(string.IsNullOrWhiteSpace(storageUrl) || string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException("Configuration is not set up");
        }

        string urlToStorage = $"{storageUrl}?code={storageKey}";

        var content = new StringContent(JsonSerializer.Serialize(storageOrder), Encoding.UTF8, "application/json");

        var result = await _httpClient.PostAsync(urlToStorage, content);
    }
}
