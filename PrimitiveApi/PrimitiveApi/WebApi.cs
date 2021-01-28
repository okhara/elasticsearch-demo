using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Nest;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PrimitiveApi
{
    public class WebApi
    {
        private readonly string _productsIndex;
        private readonly ElasticClient _elasticClient;

        public WebApi(IConfiguration configuration, ElasticClient elasticClient)
        {
            _productsIndex = configuration["ElasticSearch:Indexes:ProductsIndex"];
            _elasticClient = elasticClient;
        }

        public async Task GetProducts(HttpContext context)
        {
            var matchQuery = new MatchQuery
            {
                Field = "name",
                Query = context.Request.GetValueFromQueryString("term"),
                Fuzziness = Fuzziness.Auto
            };

            var rangeQuery = new NumericRangeQuery
            {
                Field = "price",
                GreaterThanOrEqualTo = double.TryParse(context.Request.GetValueFromQueryString("pricefrom"), out var from) ? from : (double?)null,
                LessThan = double.TryParse(context.Request.GetValueFromQueryString("priceto"), out var to) ? to : (double?)null
            };

            var request = new SearchRequest(_productsIndex)
            {
                Query = matchQuery && +rangeQuery
            };

            var response = await _elasticClient.SearchAsync<Product>(request);

            if (!response.IsValid)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                return;
            }

            await context.Response.WriteAsJsonAsync(response.Hits.Select(h => h.Source));
        }

        public async Task GetProduct(HttpContext context)
        {
            var id = context.Request.GetIdFromRouteValues();

            if (!id.HasValue)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return;
            }

            var request = new GetRequest(_productsIndex, id);

            var response = await _elasticClient.GetAsync<Product>(request);

            if (!response.IsValid)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                return;
            }

            if (!response.Found)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                return;
            }

            var product = response.Source;

            await context.Response.WriteAsJsonAsync(product);
        }

        public async Task CreateProduct(HttpContext context)
        {
            var product = await context.Request.ReadFromJsonAsync<Product>();

            if (product == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return;
            }

            var request = new IndexRequest<Product>(product, _productsIndex, product.Id);

            var response = await _elasticClient.IndexAsync(request);

            if (!response.IsValid)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.NoContent;
        }

        public async Task DeleteProduct(HttpContext context)
        {
            var id = context.Request.GetIdFromRouteValues();

            if (!id.HasValue)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return;
            }

            var request = new DeleteRequest(_productsIndex, id);

            var response = await _elasticClient.DeleteAsync(request);

            if (!response.IsValid)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                return;
            }

            if (response.Result == Result.NotFound)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.NoContent;
        }
    }
}
