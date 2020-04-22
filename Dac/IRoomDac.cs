using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Dac
{
    public interface IRoomDac
    {
        IMongoCollection<Room> Collection { get; set; }
        Task<Room> Get(Expression<Func<Room, bool>> expression);
        Task<IEnumerable<Room>> Gets(Expression<Func<Room, bool>> expression);
    }
}