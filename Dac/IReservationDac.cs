using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Dac
{
    public interface IReservationDac
    {
        IMongoCollection<Reservation> Collection { get; set; }
        Task<Reservation> Get(Expression<Func<Reservation, bool>> expression);
        Task<IEnumerable<Reservation>> Gets(FilterDefinition<Reservation> filter);
        Task Create(Reservation document);
        Task Update(Expression<Func<Reservation, bool>> expression, Reservation document);
        Task Update(Expression<Func<Reservation, bool>> expression, UpdateDefinition<Reservation> def);
    }
}