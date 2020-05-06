using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using PrompimanAPI.Models;
using PrompimanAPI.Services;

namespace PrompimanAPI.Dac
{
    public class MasterDac : IMasterDac
    {
        public IMongoCollection<Master> Collection { get; set; }

        public MasterDac(IDbService service)
        {
            Collection = service.CollectionMaster;
        }

        public async Task<bool> Any(Expression<Func<Master, bool>> expression)
            => await Collection.Find(expression).AnyAsync();

        public async Task Create(Master document)
            => await Collection.InsertOneAsync(document);

        public async Task<Master> Get(Expression<Func<Master, bool>> expression)
            => await Collection.Find(expression).FirstOrDefaultAsync();

        public async Task<IEnumerable<Master>> Gets(Expression<Func<Master, bool>> expression)
            => await Collection.Find(expression).ToListAsync();

        public async Task<IEnumerable<Master>> Gets(FilterDefinition<Master> filter, int skip, int limit)
            => await Collection.Find(filter).SortBy(x => x.CreationDateTime).Skip(skip).Limit(limit).ToListAsync();

        public async Task<IEnumerable<Master>> Gets(FilterDefinition<Master> filter)
            => await Collection.Find(filter).SortBy(x => x.CreationDateTime).ToListAsync();

        public async Task<bool> Any(FilterDefinition<Master> filter)
            => await Collection.Find(filter).AnyAsync();

        public async Task<long> Count(FilterDefinition<Master> filter)
            => await Collection.Find(filter).CountDocumentsAsync();

        public async Task Update(Expression<Func<Master, bool>> expression, Master document)
            => await Collection.ReplaceOneAsync(expression, document);

        public async Task Update(Expression<Func<Master, bool>> expression, UpdateDefinition<Master> def)
            => await Collection.UpdateOneAsync(expression, def);
    }
}