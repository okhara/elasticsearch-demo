using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace PrimitiveApi
{
    public class Program
    {
        private static IList<Product> products = new List<Product>();

        private static Task WriteAsJsonAsync<T>(HttpResponse response, T value)
        {
            response.ContentType = "application/json";

            return JsonSerializer.SerializeAsync(response.Body, value);
        }

        private static async Task<T> ReadFromJsonAsync<T>(HttpRequest request) where T : class
        {
            if (request.ContentType != "application/json")
            {
                return default;
            }

            try
            {
                return await JsonSerializer.DeserializeAsync<T>(request.Body);
            }
            catch
            {
                return default;
            }
        }

        private static Guid? GetProductId(HttpRequest request)
        {
            return request.RouteValues.TryGetValue("id", out var value) && value is string str && Guid.TryParse(str, out var id) ? id : default(Guid?);
        }

        private static Task GetProducts(HttpContext context)
        {
            return WriteAsJsonAsync(context.Response, products);
        }

        private static Task GetProduct(HttpContext context)
        {
            var id = GetProductId(context.Request);

            if (id.HasValue)
            {
                var product = products.FirstOrDefault(p => p.Id == id);

                if (product == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }

                return WriteAsJsonAsync(context.Response, product);
            }

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            return Task.CompletedTask;
        }

        private static async Task CreateProduct(HttpContext context)
        {
            var product = await ReadFromJsonAsync<Product>(context.Request);

            if (product == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return;
            }

            products.Add(product);

            context.Response.StatusCode = (int)HttpStatusCode.NoContent;
        }

        private static Task DeleteProduct(HttpContext context)
        {
            var id = GetProductId(context.Request);

            if (id.HasValue)
            {
                var product = products.FirstOrDefault(p => p.Id == id);

                if (product == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }

                products.Remove(product);

                context.Response.StatusCode = (int)HttpStatusCode.NoContent;

                return Task.CompletedTask;
            }

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            return Task.CompletedTask;
        }

        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices((context, services) =>
                    {

                    });

                    webBuilder.Configure((context, app) =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints => {
                            endpoints.MapGet("/api/products", GetProducts);
                            endpoints.MapGet("/api/products/{id}", GetProduct);
                            endpoints.MapPost("/api/products", CreateProduct);
                            endpoints.MapDelete("/api/products/{id}", DeleteProduct);

                            endpoints.MapGet("/", async context =>
                            {
                                await context.Response.WriteAsync("Hello There!");
                            });
                        });
                    });
                }).Build().Run();
        }
    }
}
