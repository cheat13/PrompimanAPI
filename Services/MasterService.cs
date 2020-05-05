using System.Linq;
using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Services
{
    public class MasterService : IMasterService
    {
        public FilterDefinition<Master> CreateFilter(string word)
        {
            var fb = Builders<Master>.Filter;
            FilterDefinition<Master> carryFilter = fb.Where(m => m.Active == true);

            if (!string.IsNullOrEmpty(word))
            {
                word = word.ToLower();
                var filter = fb.Where(m => m.Name.ToLower().Contains(word)
                    || m.GroupName.ToLower().Contains(word)
                    || m.Telephone.Contains(word)
                    || m.Rooms.Any(room => room.RoomNo == word));
                carryFilter = filter & carryFilter;
            }

            return carryFilter;
        }
    }
}