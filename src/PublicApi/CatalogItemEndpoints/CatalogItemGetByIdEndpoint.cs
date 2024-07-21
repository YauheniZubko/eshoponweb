using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using MinimalApi.Endpoint;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

/// <summary>
/// Get a Catalog Item by Id
/// </summary>
public class CatalogItemGetByIdEndpoint : IEndpoint<IResult, GetByIdCatalogItemRequest, IRepository<CatalogItem>>
{
    private readonly IUriComposer _uriComposer;

    public CatalogItemGetByIdEndpoint(IUriComposer uriComposer)
    {
        _uriComposer = uriComposer;
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("api/catalog-items/{catalogItemId}",
            async (int catalogItemId, IRepository<CatalogItem> itemRepository) =>
            {
                return await HandleAsync(new GetByIdCatalogItemRequest(catalogItemId), itemRepository);
            })
            .Produces<GetByIdCatalogItemResponse>()
            .WithTags("CatalogItemEndpoints");

        app.MapGet("api/primeCounts/{count}",
            (int count) =>
            {
                List<int> primes = new List<int>();
                int num = 0;
                var isPrime = (int num) =>
                {
                    if (num <= 1) return false;
                    for (int i = 2; i <= Math.Sqrt(num); i++)
                    {
                        if (num % i == 0) return false;
                    }
                    return true;
                };
                while (count > 0)
                {
                    if (isPrime(num))
                    {
                        count--;
                        primes.Add(num);
                    }
                    num++;
                }
                return primes.ToArray();
            }
            )
            .Produces<int[]>()
            .WithTags("PrimeCountEndpoints");
    }

    public async Task<IResult> HandleAsync(GetByIdCatalogItemRequest request, IRepository<CatalogItem> itemRepository)
    {
        var response = new GetByIdCatalogItemResponse(request.CorrelationId());

        var item = await itemRepository.GetByIdAsync(request.CatalogItemId);
        if (item is null)
            return Results.NotFound();

        response.CatalogItem = new CatalogItemDto
        {
            Id = item.Id,
            CatalogBrandId = item.CatalogBrandId,
            CatalogTypeId = item.CatalogTypeId,
            Description = item.Description,
            Name = item.Name,
            PictureUri = _uriComposer.ComposePicUri(item.PictureUri),
            Price = item.Price
        };
        return Results.Ok(response);
    }
}
