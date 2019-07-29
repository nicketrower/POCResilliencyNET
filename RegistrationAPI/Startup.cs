using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RegisterUserAPI.DAL;
using RegisterUserAPI.Repository;
using RegistrationAPI.TypedClients;
using Swashbuckle.AspNetCore.Swagger;
using TransientPolicies;
using static System.Net.Mime.MediaTypeNames;

namespace RegistrationAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Register Repository BL service
            services.AddScoped<IUserBL, UserBL>();


            //Add Entity Framework
            services.AddDbContext<GabDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("RegistrationDB"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                });
            });

            // Add Cors
            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            //Add Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Registration API", Version = "v1" });
            });

            //Define Polly Policies inline right now - Refactor after POC to put into Policy Registry Class Library. Create methods where settings such as Timeout, Exponential Retry & Circuit Breaker Sync and Advanced Sync settings can be configurable.
            PolicyDefinitions policyHelper = new PolicyDefinitions();
            var registry = services.AddPolicyRegistry();
            registry.Add("timeoutPolicy", policyHelper.timeOutPolicy(30));
            registry.Add("fallbackPolicy", policyHelper.fallBackPolicy("Circuit Breaker Invoked"));
            registry.Add("retryPolicy", policyHelper.waitAndRetry());
            registry.Add("bulkheadPolicy", policyHelper.bulkHeadIsolation(10, 2));
            registry.Add("circuitBreakerPolicy", policyHelper.circuitBreakerPolicy(3, 15));

            //Named Http Configuration, apply policies - Policies are chained to the singleton named HTTP client
            //Example below shows how to create a named HttpClient - no longer used in this POC but I left it for reference. 
            //services.AddHttpClient("EmailService", client =>
            //{
            //    client.BaseAddress = new Uri("http://notification/Message/NewUserRegistration/");
            //    client.DefaultRequestHeaders.Add("Accept", "application/json");
            //})
            //   .AddPolicyHandlerFromRegistry("bulkheadPolicy")
            //   .AddPolicyHandlerFromRegistry("timeoutPolicy")
            //   .AddPolicyHandlerFromRegistry("fallbackPolicy")
            //   .AddPolicyHandlerFromRegistry("retryPolicy")
            //   .AddPolicyHandlerFromRegistry("circuitBreakerPolicy");
       

            //Typed Singleton HttpClient Handler and Apply Polly Policies - Switched to Typed Client for the POC
            services.AddHttpClient<INotificationClient, NotificationClient>()
               .AddPolicyHandlerFromRegistry("bulkheadPolicy")
               .AddPolicyHandlerFromRegistry("timeoutPolicy")
               .AddPolicyHandlerFromRegistry("fallbackPolicy")
               .AddPolicyHandlerFromRegistry("retryPolicy")
               .AddPolicyHandlerFromRegistry("circuitBreakerPolicy");


            //Configure Health Check API - Heartbeat Probe on service dependencies
            services.AddHealthChecks()
                  .AddApplicationInsightsPublisher()
                  .AddDbContextCheck<GabDbContext>("Registration SQL")
                  .AddUrlGroup(new Uri("http://notification"), "NotificationAPI");

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Registration API");
                c.RoutePrefix = string.Empty;
            });

            //Access health API via http://localhost:8081/health - Can be used to read the JSON and not invoke/retry a resource if it's unhealthy. Typical Use Case these are downstream and you would not know proactively without a heartbeat pulse check. This provides a proactive approach. 
            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            //Spins up a database in the linux container on build
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<GabDbContext>();
                context.Database.Migrate();
            }
            app.UseMvc();
        }
    }
}
