using System.Threading.Tasks;
namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;
public interface IAzureStorageService
{
    Task AddOrder(int basketId);
}
