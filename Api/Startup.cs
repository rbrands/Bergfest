using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Cosmos;
using BlazorApp.Api.Repositories;
using BlazorApp.Shared;
using Microsoft.AspNetCore.Routing;
using System.Xml.Linq;
using Flurl;
using Flurl.Http.Configuration;
using System.Reflection;

[assembly: FunctionsStartup(typeof(BlazorApp.Api.Startup))]
namespace BlazorApp.Api
{
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// To use a static Cosmos DB client create class for Dependency Injection.
        /// See: https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
        /// and https://towardsdatascience.com/working-with-azure-cosmos-db-in-your-azure-functions-cc4f0f98a44d
        /// and https://blog.rasmustc.com/azure-functions-dependency-injection/
        /// </summary>
        /// <param name="builder"></param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddFilter(level => true);
            });



            IConfiguration config = new ConfigurationBuilder()
                           .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                           .AddUserSecrets(Assembly.GetExecutingAssembly())
                           .AddEnvironmentVariables()
                           .Build();

            builder.Services.AddSingleton(config);
            CosmosClient cosmosClient = new CosmosClient(config["COSMOS_DB_CONNECTION_STRING"]);
            builder.Services.AddSingleton(cosmosClient);
            builder.Services.AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>();
            builder.Services.AddSingleton<StravaRepository>(); 
        }
    }
}
