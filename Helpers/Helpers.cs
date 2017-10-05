using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace PodcastsSyndicate
{
    public static class Helpers
    {
        public static IConfiguration Configuration { get; set; }
    }

    public static class Extensions
    {
        public static HttpRequestMessage CreateProxyHttpRequest(this HttpContext context, string uriString)
        {
            var uri = new Uri(uriString);
            var request = context.Request;

            var requestMessage = new HttpRequestMessage();
            var requestMethod = request.Method;
            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(request.Body);
                requestMessage.Content = streamContent;
            }

            // Copy the request headers
            foreach (var header in request.Headers)
            {
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
                {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            requestMessage.Headers.Host = uri.Authority;
            requestMessage.RequestUri = uri;
            requestMessage.Method = new HttpMethod(request.Method);

            return requestMessage;
        }

        public static Task<HttpResponseMessage> SendProxyHttpRequest(this HttpContext context, string proxiedAddress)
        {
            var proxiedRequest = context.CreateProxyHttpRequest(proxiedAddress);
            return new HttpClient().SendAsync(proxiedRequest, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
        }
        
        public static async Task CopyProxyHttpResponse(this HttpContext context, HttpResponseMessage responseMessage)
        {
            var response = context.Response;

            response.StatusCode = (int)responseMessage.StatusCode;
            foreach (var header in responseMessage.Headers)
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }

            // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
            response.Headers.Remove("transfer-encoding");

            using (var responseStream = await responseMessage.Content.ReadAsStreamAsync())
            {
                await responseStream.CopyToAsync(response.Body, 81920, context.RequestAborted);
            }
        }
        
        public static void UseProxy(this IApplicationBuilder app, string endpoint, Func<string[], Task<string>> getProxiedAddress)
        {
            app.MapWhen(context => {
                var path = context.Request.Path.Value;
                var match = Regex.Match(path, endpoint);

                return match.Success;
            }, ap => {
                ap.Run(async context => {
                    try
                    {
                        var path = context.Request.Path.Value;
                        var match = Regex.Match(path, endpoint);
                        var vars = match.Groups.Skip(1).Select(g => g.Value).ToArray();

                        var proxiedAddress = await getProxiedAddress(vars);

                        var proxiedResponse = await context.SendProxyHttpRequest(proxiedAddress);
                        
                        await context.CopyProxyHttpResponse(proxiedResponse);
                    }
                    catch(Exception)
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Request could not be proxied.");
                    }
                });
            });
        }
    }
}