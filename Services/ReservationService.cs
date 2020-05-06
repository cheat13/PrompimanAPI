using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Services
{
    public class ReservationService : IReservationService
    {
        public FilterDefinition<Reservation> CreateFilter(string word)
        {
            var fb = Builders<Reservation>.Filter;
            FilterDefinition<Reservation> carryFilter = fb.Where(r => r.Active == true);

            if (!string.IsNullOrEmpty(word))
            {
                word = word.ToLower();
                var filter = fb.Where(r => r.Name.Contains(word) || r.Telephone.Contains(word));
                carryFilter = filter & carryFilter;
            }

            return carryFilter;
        }
    }
}