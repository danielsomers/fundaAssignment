using FundaAssignment;
using Microsoft.Extensions.Http;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddHttpClient();
        services.AddTransient<ISearchService, SearchService>();
    })
    .Build();

host.Run();