using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Services
{
    public interface IRoomActService
    {
        Task Upsert(CreateRoomActRequest req, DateTime time);
        Task Delete(FilterDefinition<RoomActivated> filter, DateTime time);
        Task<IEnumerable<RoomActivated>> Create(CreateRoomActRequest req, string status, DateTime time);
        IEnumerable<RoomActivated> SetSelected(IEnumerable<RoomActivated> roomActLst, string roomId = null);
        CalculateExpense CalculateExpense(IEnumerable<Expense> expenseList, DateTime time);
    }
}