using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrompimanAPI.Models;

namespace PrompimanAPI.Services
{
    public interface IRoomActService
    {
        Task<IEnumerable<RoomActivated>> CreateRoomActLst(CreateRoomActRequest req, string status, DateTime time);
    }
}