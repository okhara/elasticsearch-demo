using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PrimitiveApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices((context, services) =>
                    {
                        services.AddSingleton(new ElasticClient(new Uri(context.Configuration["ElasticSearch:ConnectionUri"])));
                        services.AddScoped<WebApi>();
                    });

                    webBuilder.Configure((context, app) =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/api/products", context => context.Resolve<WebApi>().GetProducts(context));
                            endpoints.MapGet("/api/products/{id}", context => context.Resolve<WebApi>().GetProduct(context));
                            endpoints.MapPost("/api/products", context => context.Resolve<WebApi>().CreateProduct(context));
                            endpoints.MapPut("/api/products", context => context.Resolve<WebApi>().CreateProduct(context));
                            endpoints.MapDelete("/api/products/{id}", context => context.Resolve<WebApi>().DeleteProduct(context));
                        });
                    });
                }).Build().Run();
        }
    }
}
