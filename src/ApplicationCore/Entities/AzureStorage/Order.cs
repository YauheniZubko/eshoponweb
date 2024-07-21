namespace Microsoft.eShopWeb.ApplicationCore.Entities.AzureStorage;
public class Order
{
    public string BuyerId { get; set; }
    public OrderItem[] OrderedItems { get; set; }
}
