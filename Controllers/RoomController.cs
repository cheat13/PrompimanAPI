using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        public IMongoCollection<Room> CollectionRoom { get; set; }
        public IMongoCollection<RoomActivated> CollectionRoomActivated { get; set; }

        public RoomController()
        {
            var client = new MongoClient("mongodb://firstclass:Th35F1rstCla55@104.215.253.165/hotel");
            var database = client.GetDatabase("hotel");

            CollectionRoom = database.GetCollection<Room>("room");
            CollectionRoomActivated = database.GetCollection<RoomActivated>("roomActivated");
        }

        [HttpPut]
        public async Task<IEnumerable<Room>> Get(DateRequest req)
        {
            var rooms = await CollectionRoom.Find(r => true).ToListAsync();

            var filter = CreateFilter(req.CheckInDate, req.CheckOutDate);
            var roomActLst = await CollectionRoomActivated.Find(filter).ToListAsync();
            if (roomActLst.Any())
            {
                var roomNoActLst = roomActLst.Select(it => it.RoomNo).ToList();

                rooms.ForEach(room =>
                {
                    var roomAct = roomActLst.FirstOrDefault(r => r.RoomNo == room._id);
                    if (roomAct != null)
                    {
                        room.Status = roomAct.Status;
                    }
                });
            }

            return rooms;
        }

        private static FilterDefinition<RoomActivated> CreateFilter(DateTime checkInDate, DateTime checkOutDate)
        {
            var fb = Builders<RoomActivated>.Filter;
            FilterDefinition<RoomActivated> carryFilter = fb.Where(r => r.Active == true);

            var filter = fb.Where(r => !((r.ArrivalDate - checkOutDate).TotalHours >= 17 || (checkInDate - r.Departure).TotalHours >= 17));
            carryFilter = filter & carryFilter;

            return carryFilter;
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

        //     await CollectionRoom.InsertManyAsync(rooms);

        //     return new Response
        //     {
        //         IsSuccess = true
        //     };
        // }
    }
}