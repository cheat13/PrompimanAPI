using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using PrompimanAPI.Models;
using PrompimanAPI.Services;

namespace PrompimanAPI.Dac
{
    public class MemberDac : IMemberDac
    {
        public IMongoCollection<Member> Collection { get; set; }

        public MemberDac(IDbService service)
        {
            Collection = service.CollectionMember;
        }

        public async Task<Member> Get(Expression<Func<Member, bool>> expression)
            => await Collection.Find(expression).FirstOrDefaultAsync();

        public async Task<IEnumerable<Member>> Gets(FilterDefinition<Member> filter, int skip, int limit)
            => await Collection.Find(filter).SortBy(x => x._id).Skip(skip).Limit(limit).ToListAsync();

        public async Task Create(Member document)
            => await Collection.InsertOneAsync(document);

        public async Task Update(Expression<Func<Member, bool>> expression, Member document)
            => await Collection.ReplaceOneAsync(expression, document);

        public async Task<long> Count(FilterDefinition<Member> filter)
            => await Collection.Find(filter).CountDocumentsAsync();

        public async Task<bool> Any(FilterDefinition<Member> filter)
            => await Collection.Find(filter).AnyAsync();
    }
}