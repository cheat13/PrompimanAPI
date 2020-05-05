using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Services
{
    public interface IMemberService
    {
        FilterDefinition<Member> CreateFilter(string word);
    }
}