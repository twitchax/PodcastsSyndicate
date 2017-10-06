using AspNetCore.Proxy;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PodcastsSyndicate.Dal;

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
            services.AddCors();

            services.AddMvc().AddXmlSerializerFormatters().AddJsonOptions(options => {
                options.SerializerSettings.Formatting = Formatting.Indented;
            });

            services.AddApplicationInsightsTelemetry(Configuration);
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
            
            app.UseCors(builder =>
                builder.WithOrigins("*").AllowAnyHeader()
            );
            
            app.Use(HandleSubdomain);
            app.Use(HandleAuthorization);

            app.UseProxies();
            
            app.UseMvc();
        }

        private async Task HandleAuthorization(HttpContext context, Func<Task> next)
        {
            var authSplits = context.Request.Headers["Authorization"].FirstOrDefault()?.ToString().Split(" ");

            if(authSplits != null && authSplits.Length == 3 && authSplits[0] == "Bearer")
            {
                var email = authSplits[1];
                var authToken = authSplits[2];

                var user = await Db.Users.Document(email).ReadAsync();

                if(user != null && user.AuthToken == authToken)
                    context.Items.Add("User", user);
            }

            await next.Invoke();
        }

        private async Task HandleSubdomain(HttpContext context, Func<Task> next)
        {
            string url = context.Request.Host.Value.ToLower(); 
            var splits = url.Split('.');

            if (!splits[0].Contains("localhost") && !splits[0].Contains("podcastssyndicate") && !splits[0].Contains("www") && !splits[0].Contains("content"))
            {
                if(splits[0] == "rss")
                {
                    var podcastName = splits[1];
                    context.Request.Path = new PathString($"/podcast/{podcastName}/rss").Add(context.Request.Path);
                }
                else
                {
                    var podcastName = splits[0];
                    context.Request.Path = new PathString($"/podcast/{podcastName}").Add(context.Request.Path);
                }
            }

            await next.Invoke();
        }
    }
}
