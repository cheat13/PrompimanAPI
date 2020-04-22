using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using PrompimanAPI.Models;
using PrompimanAPI.Services;

namespace PrompimanAPI.Dac
{
    public class RoomDac : IRoomDac
    {
        public IMongoCollection<Room> Collection { get; set; }

        public RoomDac(IDbService service)
        {
            Collection = service.CollectionRoom;
        }

        public async Task<Room> Get(Expression<Func<Room, bool>> expression)
            => await Collection.Find(expression).FirstOrDefaultAsync();

        public async Task<IEnumerable<Room>> Gets(Expression<Func<Room, bool>> expression)
            => await Collection.Find(expression).ToListAsync();
    }
}