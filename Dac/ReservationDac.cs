using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using PrompimanAPI.Models;
using PrompimanAPI.Services;

namespace PrompimanAPI.Dac
{
    public class ReservationDac : IReservationDac
    {
        public IMongoCollection<Reservation> Collection { get; set; }

        public ReservationDac(IDbService service)
        {
            Collection = service.CollectionReservation;
        }

        public async Task<Reservation> Get(Expression<Func<Reservation, bool>> expression)
            => await Collection.Find(expression).FirstOrDefaultAsync();

        public async Task<IEnumerable<Reservation>> Gets(FilterDefinition<Reservation> filter)
            => await Collection.Find(filter).SortBy(x => x.CheckInDate).ToListAsync();

        public async Task Create(Reservation document)
            => await Collection.InsertOneAsync(document);

        public async Task Update(Expression<Func<Reservation, bool>> expression, Reservation document)
            => await Collection.ReplaceOneAsync(expression, document);

        public async Task Update(Expression<Func<Reservation, bool>> expression, UpdateDefinition<Reservation> def)
            => await Collection.UpdateOneAsync(expression, def);
    }
}