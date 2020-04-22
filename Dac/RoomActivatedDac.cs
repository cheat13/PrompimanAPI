using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using PrompimanAPI.Models;
using PrompimanAPI.Services;

namespace PrompimanAPI.Dac
{
    public class RoomActivatedDac : IRoomActivatedDac
    {
        public IMongoCollection<RoomActivated> Collection { get; set; }

        public RoomActivatedDac(IDbService service)
        {
            Collection = service.CollectionRoomActivated;
        }

        public async Task<RoomActivated> Get(Expression<Func<RoomActivated, bool>> expression)
            => await Collection.Find(expression).FirstOrDefaultAsync();

        public async Task<IEnumerable<RoomActivated>> Gets(Expression<Func<RoomActivated, bool>> expression)
            => await Collection.Find(expression).ToListAsync();

        public async Task<IEnumerable<RoomActivated>> Gets(FilterDefinition<RoomActivated> filter)
            => await Collection.Find(filter).ToListAsync();

        public async Task Creates(IEnumerable<ReplaceOneModel<RoomActivated>> writeModels)
            => await Collection.BulkWriteAsync(writeModels);

        public async Task Updates(FilterDefinition<RoomActivated> filter, UpdateDefinition<RoomActivated> def)
            => await Collection.UpdateManyAsync(filter, def);
    }
}