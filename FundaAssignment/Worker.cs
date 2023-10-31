using Object = FundaAssignment.Models.Object;

namespace FundaAssignment;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ISearchService _searchService;

    public Worker(ILogger<Worker> logger, ISearchService searchService)
    {
        _logger = logger;
        _searchService = searchService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
       var results = await _searchService.GetAllResults(new List<string>() { "amsterdam" });
       var resultsWithTuin = await _searchService.GetAllResults(new List<string>() { "amsterdam", "tuin" });

       DisplayResults(results, "Top 10 Makelaar in Amsterdam");
       
       DisplayResults(resultsWithTuin, "Top 10 Makelaar in Amsterdam With Tuin");

    }

    private void DisplayResults(List<Object> results, string titleMessage)
    {
        Console.WriteLine("# " + titleMessage + " #");
        int rankNumber = 1;
        foreach(var line in results.GroupBy(x => x.MakelaarId)
                    .Select(group => new { 
                        MakelaarName = group.FirstOrDefault()?.MakelaarNaam, 
                        Count = group.Count() 
                    })
                    .OrderByDescending(x => x.Count).Take(10))
        {
            Console.WriteLine($"Rank: {rankNumber} Name: {line.MakelaarName} Total: {line.Count}");
            rankNumber++;
        }
        Console.WriteLine();
    }
}
