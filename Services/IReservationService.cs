using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Services
{
    public interface IReservationService
    {
        FilterDefinition<Reservation> CreateFilter(string word);
    }
}