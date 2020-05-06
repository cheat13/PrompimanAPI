using System.Threading.Tasks;
using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Services
{
    public interface IMemberService
    {
        Task<DataPaging<Member>> GetDataPaging(int page, int size, string word);
    }
}