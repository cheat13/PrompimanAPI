using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Dac
{
    public interface IRoomActivatedDac
    {
        IMongoCollection<RoomActivated> Collection { get; set; }
        Task<RoomActivated> Get(Expression<Func<RoomActivated, bool>> expression);
        Task<IEnumerable<RoomActivated>> Gets(Expression<Func<RoomActivated, bool>> expression);
        Task<IEnumerable<RoomActivated>> Gets(FilterDefinition<RoomActivated> filter);
        Task Creates(IEnumerable<ReplaceOneModel<RoomActivated>> writeModels);
        Task Updates(FilterDefinition<RoomActivated> filter, UpdateDefinition<RoomActivated> def);

    }
}