using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Services
{
    public interface IMasterService
    {
        Task<DataPaging<MasterInfo>> GetDataPaging(int page, int size, string word, bool active = true);
        Task<DataPaging<MasterInfo>> GetAllCheckOut(int page, int size, string word, bool haveRemaining);
    }
}