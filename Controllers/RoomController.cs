using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using MongoDB.Driver;
using PrompimanAPI.Dac;
using PrompimanAPI.Models;
using PrompimanAPI.Services;

namespace PrompimanAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomDac roomDac;
        private readonly IRoomActivatedDac roomActivatedDac;

        public RoomController(
            IRoomDac roomDac,
            IRoomActivatedDac roomActivatedDac)
        {
            this.roomDac = roomDac;
            this.roomActivatedDac = roomActivatedDac;
        }

        [HttpPut]
        public async Task<IEnumerable<Room>> Get(DateRequest req)
        {
            var rooms = await roomDac.Gets(x => true);
            var roomLst = rooms.ToList();

            var roomActLst = await roomActivatedDac.Gets(x => true);
            var qryRoomActLst = roomActLst.Where(r => !((r.ArrivalDate - req.CheckOutDate).TotalHours >= 18 || (req.CheckInDate - r.Departure).TotalHours >= 18));

            if (qryRoomActLst.Any())
            {
                roomLst.ForEach(room =>
                {
                    var roomAct = qryRoomActLst.FirstOrDefault(r => r.RoomNo == room._id);
                    if (roomAct != null) room.Status = roomAct.Status;
                });
            }

            return roomLst;
        }

        // [HttpPost]
        // public async Task<Response> InsertRoomdata()
        // {
        //     var rooms = new List<Room>();
        //     using (var reader = new StreamReader(@"C:/Users/trago/OneDrive/Desktop/room.txt"))
        //     {
        //         while (!reader.EndOfStream)
        //         {
        //             var line = reader.ReadLine();
        //             var values = line.Split(',');

        //             var room = new Room();
        //             room._id = values[0];

        //             if (int.TryParse(values[1], out int roomType))
        //             {
        //                 room.RoomType = (RoomType)(int.Parse(values[1]));
        //             }
        //             if (int.TryParse(values[2], out int bedType))
        //             {
        //                 room.BedType = (BedType)(int.Parse(values[2]));
        //             }
        //             if (int.TryParse(values[3], out int rate))
        //             {
        //                 room.Rate = int.Parse(values[3]);
        //             }
        //             room.Status = "ว่าง";

        //             rooms.Add(room);
        //         }
        //     }

        //     await dbService.CollectionRoom.InsertManyAsync(rooms);

        //     return new Response
        //     {
        //         IsSuccess = true
        //     };
        // }
    }
}