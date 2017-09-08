using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace PodcastsSyndicate
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Helpers.Configuration = Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddJsonOptions(options => {
                options.SerializerSettings.Formatting = Formatting.Indented;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            
            // Rewrite URLs with subdomains into the proper form (i.e., "mypodcast.podcastssyndicate.com/rss" +> "podcastssyndicate.com/podcast/mypodcast/rss").
            app.Use(HandleSubdomain);

            // TODO: Add an auth handler here?  Shove in HttpContext.Items?

            app.UseMvc();
        }

        private async Task HandleSubdomain(HttpContext context, Func<Task> next)
        {
            string url = context.Request.Host.Value.ToLower(); 
            var splits = url.Split('.');

            if (!splits[0].Contains("localhost") && !splits[0].Contains("podcastssyndicate") && !splits[0].Contains("www"))
            {
                var podcastName = splits[0];

                // Reqrite the path to include the 
                context.Request.Path = new PathString($"/podcast/{podcastName}").Add(context.Request.Path);
            }

            await next.Invoke();
        }
    }
}
