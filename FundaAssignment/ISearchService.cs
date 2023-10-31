using FundaAssignment.Models;
using Object = FundaAssignment.Models.Object;

namespace FundaAssignment;

public interface ISearchService
{
    Task<List<Object>> GetAllResults(List<string> searchTerms);
}