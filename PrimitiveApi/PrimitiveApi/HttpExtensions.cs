using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PrimitiveApi
{
    public static class HttpExtensions
    {
        public static T Resolve<T>(this HttpContext context)
        {
            return context.RequestServices.GetRequiredService<T>();
        }

        public static Task WriteAsJsonAsync<T>(this HttpResponse response, T value)
        {
            response.ContentType = "application/json";

            return JsonSerializer.SerializeAsync(response.Body, value);
        }

        public static async Task<T> ReadFromJsonAsync<T>(this HttpRequest request) where T : class
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

        public static Guid? GetIdFromRouteValues(this HttpRequest request)
        {
            return request.RouteValues.TryGetValue("id", out var value) && value is string str && Guid.TryParse(str, out var id) ? id : default(Guid?);
        }

        public static string GetValueFromQueryString(this HttpRequest request, string key)
        {
            return request.Query.TryGetValue(key, out var values) ? values.Where(v => !string.IsNullOrWhiteSpace(v)).FirstOrDefault() : null;
        }
    }
}
