using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Dac
{
    public interface IMemberDac
    {
        IMongoCollection<Member> Collection { get; set; }
        Task<Member> Get(Expression<Func<Member, bool>> expression);
        Task<IEnumerable<Member>> Gets(FilterDefinition<Member> filter, int skip, int limit);
        Task Create(Member document);
        Task Update(Expression<Func<Member, bool>> expression, Member document);
        Task<long> Count(FilterDefinition<Member> filter);
        Task<bool> Any(FilterDefinition<Member> filter);
    }
}