using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.CosmosDb;
public class Order
{
    public string id { get; set; }
    public Address Address { get; set; }
    public OrderItem[] OrderedItems { get; set; }
    public decimal Price { get; set; }
}
