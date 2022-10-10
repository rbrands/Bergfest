using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Cosmos;
using BlazorApp.Shared;
using System.Reflection;
using Flurl.Http.Configuration;
using BackendLibrary;


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
            CosmosClient cosmosClient = new CosmosClient(builder.GetContext().Configuration["COSMOS_DB_CONNECTION_STRING"]);
            builder.Services.AddSingleton(cosmosClient);
            builder.Services.AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>();
            builder.Services.AddSingleton<StravaRepository>();
            builder.Services.AddSingleton<CosmosDBRepository<StravaSegmentEffort>>();
            builder.Services.AddSingleton<CosmosDBRepository<StravaSegment>>();
            builder.Services.AddSingleton<QueueStorageRepository>();
            builder.Services.AddSingleton<CosmosDBRepository<StravaSegmentChallenge>>();
            builder.Services.AddSingleton<ChallengeRepository>();
        }
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            builder.ConfigurationBuilder
                           .SetBasePath(context.ApplicationRootPath)
                           .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
                           .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true, reloadOnChange: true)
                           .AddEnvironmentVariables();
        }
    }
}
