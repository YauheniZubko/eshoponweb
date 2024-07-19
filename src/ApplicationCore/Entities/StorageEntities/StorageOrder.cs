namespace Microsoft.eShopWeb.ApplicationCore.Entities.StorageEntities;
public class StorageOrder
{
    public string BuyerId { get; set; }
    public StorageOrderItemInfo[] OrderedItems { get; set; }
}
