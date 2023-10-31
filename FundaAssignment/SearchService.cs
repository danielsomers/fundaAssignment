using System.Net;

namespace FundaAssignment;
using Microsoft.Extensions.Logging;
using FundaAssignment.Models;
using Newtonsoft.Json;

public sealed class SearchService : ISearchService
{
    private readonly IHttpClientFactory _httpClientFactory = null!;
    private readonly ILogger<SearchService> _logger = null!;
    private readonly IConfiguration _config;
    private HttpStatusCode _statusCode;
    private const string BaseAddress = "http://partnerapi.funda.nl/feeds/Aanbod.svc/json/";
    // You would want to store this in a config file, or as a secret if this were for prd
    private const int PageSize = 25;

    public SearchService(
        IHttpClientFactory httpClientFactory,
        ILogger<SearchService> logger,
        IConfiguration config) =>
        (_httpClientFactory, _logger, _config) = (httpClientFactory, logger, config);

    public async Task<List<Object>> GetAllResults(List<string> searchTerms)
    {
        var searchStr = string.Empty;
        if (searchTerms.Count != 0)
        {
            searchStr = "&zo=/";
            foreach (var searchTerm in searchTerms)
            {
                searchStr += searchTerm + "/";
            }
        }
        
        using HttpClient client = _httpClientFactory.CreateClient();
        var morePages = false;
        var pageCounter = 1;
        List<Object> requestResult = new List<Object>();
        var numberOfPages = 1;
        int retryAttemptNumber = 1;
        do
        {
            var result =  await Request(client, searchStr, pageCounter);
            if (pageCounter == 1)
            {
                numberOfPages = result.TotaalAantalObjecten / PageSize;
                numberOfPages += result.TotaalAantalObjecten % PageSize == 0 ? 0 : 1;
            }

            if (_statusCode == HttpStatusCode.OK)
            {
                requestResult.AddRange(result.Objects);
                pageCounter++;
                retryAttemptNumber = 1;
            }
            else
            {
                // first retry is 1 min 1 second,
                // then retries increase to the ^2 of that time
                var retrySeconds = 61 * retryAttemptNumber ^ 2;
                _logger.LogWarning("Status: " + _statusCode + " Backing off for: " + retrySeconds + " seconds");
                await Task.Delay(1000 * retrySeconds);
            }

            morePages = pageCounter != numberOfPages;
            
        } while (morePages);
        
        return requestResult;
    }

    private async Task<Result> Request(HttpClient client, string searchStr, int page)
    {
        try
        {
            Result? result = null;
            await client.GetAsync(
                BaseAddress + _config.GetValue<string>("KEY") +$"/?type=koop{searchStr}&page={page}&pagesize={PageSize}"
            ).ContinueWith((taskWithResponse) =>
            {
                var response = taskWithResponse.Result;
                _statusCode = response.StatusCode;
                var jsonString = response.Content.ReadAsStringAsync();
                jsonString.Wait();
                result = JsonConvert.DeserializeObject<Result>(jsonString.Result);
            });
            
            return result ?? new Result();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error {Error}", ex);
        }

        return new Result();
        
    }
    
}