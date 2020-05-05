using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Services
{
    public interface IMasterService
    {
        FilterDefinition<Master> CreateFilter(string word);
    }
}