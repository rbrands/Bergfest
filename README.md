# Bergfest

Bergfest is a tool to analyze segments on [Strava](https://www.strava.com) for competetive riding. It uses the [Strava API](https://developers.strava.com/) and follows the [Strava API Agreement](https://www.strava.com/legal/api) and the [Strava Brand Guidelines](https://developers.strava.com/guidelines).

Bergfest is a [Azure Static Web App](https://learn.microsoft.com/en-us/azure/static-web-apps/overview) with [Blazor](https://blazor.net) as client technology. As starting point the [template](https://github.com/staticwebdev/blazor-starter) is used. 

## Architecture

The following diagram shows the overall architecture:
![Architecture](/docs/Bergfest.png)

## Solution Structure

- **Client**: The Blazor WebAssembly application
- **Api**: The C# Azure Functions API, which the Blazor application will call
- **Shared**: A C# class library with a shared data model between the Blazor and Functions application
- **Bergfest-Webhook**: A C# Azure Functions API, essentially with the Webhook to be called by Strava
- **BackenLibrary**: Repositories (like access to Cosmos DB) to be used by "Api" and "Bergfest-Webhook"

## Deploy to Azure Static Web Apps

This application can be deployed to [Azure Static Web Apps](https://docs.microsoft.com/azure/static-web-apps), to learn how, check out [our quickstart guide](https://aka.ms/blazor-swa/quickstart).
The following configuration values have to be set as environment variables (in Api and Bergfest-Webhook):
- tbd
