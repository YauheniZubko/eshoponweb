using System.Threading.Tasks;
namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;
public interface IStorageService
{
    Task AddToOrderItemsReserverStorage(int basketId);
}
