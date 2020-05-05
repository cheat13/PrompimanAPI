using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Dac
{
    public interface IMasterDac
    {
        IMongoCollection<Master> Collection { get; set; }
        Task<bool> Any(Expression<Func<Master, bool>> expression);
        Task<bool> Any(FilterDefinition<Master> filter);
        Task<long> Count(FilterDefinition<Master> filter);
        Task<Master> Get(Expression<Func<Master, bool>> expression);
        Task<IEnumerable<Master>> Gets(FilterDefinition<Master> filter, int skip, int limit);
        Task Create(Master document);
        Task Update(Expression<Func<Master, bool>> expression, Master document);
        Task Update(Expression<Func<Master, bool>> expression, UpdateDefinition<Master> def);
    }
}