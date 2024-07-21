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
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Services;
public class DeliveryOrderProcessorService : ICosmosDbService
{
    private readonly IRepository<Basket> _basketRepository;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public DeliveryOrderProcessorService(HttpClient httpClient,
        IRepository<Basket> basketRepository,
        IRepository<CatalogItem> itemRepository,
        IUriComposer uriComposer,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _basketRepository = basketRepository;
        _configuration = configuration;
    }

    public async Task AddOrder(int basketId, Address address)
    {
        var basketSpec = new BasketWithItemsSpecification(basketId);
        var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);

        Guard.Against.Null(basket, nameof(basket));
        Guard.Against.EmptyBasketOnCheckout(basket.Items);

        var orderedItems = basket.Items
            .Select(item => new Entities.CosmosDb.OrderItem
            {
                CatalogItemId = item.Id,
                Quantity = item.Quantity
            }).ToArray();

        var storageOrder = new Entities.CosmosDb.Order()
        {
            Address = address,
            OrderedItems = orderedItems,
            Price = basket.Items.Sum(item => item.Quantity * item.UnitPrice)
        };

        string storageUrl = _configuration["DeliveryOrderProcessor:Url"] ?? "";
        string storageKey = _configuration["DeliveryOrderProcessor:Key"] ?? "";

        if (string.IsNullOrWhiteSpace(storageUrl) || string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException("Configuration is not set up");
        }

        string urlToStorage = $"{storageUrl}?code={storageKey}";

        var content = new StringContent(JsonSerializer.Serialize(storageOrder), Encoding.UTF8, "application/json");

        var result = await _httpClient.PostAsync(urlToStorage, content);
    }
}
