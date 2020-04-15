using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        public IMongoCollection<Reservation> CollectionReservation { get; set; }
        public IMongoCollection<RoomActivated> CollectionRoomActivated { get; set; }
        public IMongoCollection<Room> CollectionRoom { get; set; }

        public ReservationController()
        {
            var client = new MongoClient("mongodb://firstclass:Th35F1rstCla55@104.215.253.165/hotel");
            var database = client.GetDatabase("hotel");

            CollectionReservation = database.GetCollection<Reservation>("reservation");
            CollectionRoomActivated = database.GetCollection<RoomActivated>("roomActivated");
            CollectionRoom = database.GetCollection<Room>("room");
        }

        [HttpGet]
        public async Task<IEnumerable<Reservation>> Get(string word)
        {
            var filter = CreateFilter(word);
            return await CollectionReservation.Find(filter).SortBy(x => x.CheckInDate).ToListAsync();
        }

        private static FilterDefinition<Reservation> CreateFilter(string word)
        {
            var fb = Builders<Reservation>.Filter;
            FilterDefinition<Reservation> carryFilter = fb.Where(r => r.Active == true);

            if (!string.IsNullOrEmpty(word))
            {
                word = word.ToLower();
                var filter = fb.Where(r => r.Name.Contains(word) || r.Telephone.Contains(word));
                carryFilter = filter & carryFilter;
            }

            return carryFilter;
        }

        [HttpGet("{id}")]
        public async Task<Reservation> GetById(string id)
        {
            return await CollectionReservation.Find(r => r._id == id).FirstOrDefaultAsync();
        }

        [HttpPost]
        public async Task<Response> Create([FromBody] Reservation res)
        {
            var now = DateTime.Now;

            // Create Reservation
            res._id = Guid.NewGuid().ToString();
            res.CreationDateTime = now;
            res.LastUpdate = now;
            res.Active = true;

            await CollectionReservation.InsertOneAsync(res);

            // Create RoomActivated

            var roomNoLst = res.Rooms.Select(r => r.RoomNo).ToList();
            var rooms = await CollectionRoom.Find(r => roomNoLst.Contains(r._id)).ToListAsync();

            var roomActLst = res.Rooms.Select(it =>
                {
                    var room = rooms.First(r => r._id == it.RoomNo);

                    return new RoomActivated
                    {
                        _id = Guid.NewGuid().ToString(),
                        GroupId = res._id,
                        RoomNo = room._id,
                        RoomType = room.RoomType,
                        BedType = room.BedType,
                        Rate = room.Rate,
                        ArrivalDate = res.CheckInDate,
                        Departure = res.CheckOutDate,
                        Setting = it.Setting,
                        Status = "จอง",
                        Active = true,
                        CreationDateTime = now,
                        LastUpdate = now,
                    };
                }).ToList();

            await CollectionRoomActivated.InsertManyAsync(roomActLst);

            return new Response
            {
                IsSuccess = true
            };
        }

        [HttpPut("{id}")]
        public async Task<Response> Update(string id, int addReserve, [FromBody] Reservation res)
        {
            var now = DateTime.Now;

            // Update Reservation
            var defUpdateRes = Builders<Reservation>.Update
                .Set(r => r.Name, res.Name)
                .Set(r => r.Telephone, res.Telephone)
                .Set(r => r.CheckInDate, res.CheckInDate)
                .Set(r => r.CheckOutDate, res.CheckOutDate)
                .Set(r => r.Rooms, res.Rooms)
                .Set(r => r.Reserve, res.Reserve + addReserve)
                .Set(r => r.LastUpdate, now);

            await CollectionReservation.UpdateOneAsync(r => r._id == id, defUpdateRes);

            var roomActLst = await CollectionRoomActivated.Aggregate()
                .Match(x => x.GroupId == id)
                .Project(x => new
                {
                    _id = x._id,
                    roomNo = x.RoomNo
                })
                .ToListAsync();

            var oldRoomNoLst = roomActLst.Select(r => r.roomNo).ToList();
            var newRoomNoLst = res.Rooms.Select(r => r.RoomNo).ToList();

            // Update RoomActivated
            var updateRooms = newRoomNoLst.Intersect(oldRoomNoLst);
            foreach (var roomNo in updateRooms)
            {
                var arrivalDate = res.CheckInDate;
                var departure = res.CheckOutDate;
                var setting = res.Rooms.First(r => r.RoomNo == roomNo).Setting;

                var defUpdateRoom = Builders<RoomActivated>.Update
                    .Set(r => r.ArrivalDate, arrivalDate)
                    .Set(r => r.Departure, departure)
                    .Set(r => r.Setting, setting)
                    .Set(r => r.LastUpdate, now);

                var roomActId = roomActLst.First(r => r.roomNo == roomNo)._id;

                await CollectionRoomActivated.UpdateOneAsync(r => r._id == roomActId, defUpdateRoom);
            }

            // Delete RoomActivated
            var removeRooms = oldRoomNoLst.Except(newRoomNoLst);
            if (removeRooms.Any()) await DeleteRoomAct(id, now, removeRooms);

            // Create RoomActivated
            var addRooms = newRoomNoLst.Except(oldRoomNoLst);
            if (addRooms.Any())
            {
                var rooms = res.Rooms.Where(r => addRooms.Contains(r.RoomNo)).ToList();
                await CreateRoomAct(rooms, id, res.CheckInDate, res.CheckOutDate, now);
            }

            return new Response
            {
                IsSuccess = true,
            };
        }

        [HttpPut("{id}")]
        public async Task<Response> Delete(string id, string note)
        {
            var now = DateTime.Now;

            // Delete Reservation
            var def = Builders<Reservation>.Update
                .Set(r => r.Active, false)
                .Set(r => r.Note, note)
                .Set(r => r.LastUpdate, now);

            await CollectionReservation.UpdateOneAsync(r => r._id == id, def);

            // Delete RoomActivated
            await DeleteRoomAct(id, now);

            return new Response
            {
                IsSuccess = true,
            };
        }

        private async Task CreateRoomAct(IEnumerable<RoomSelected> rooms, string groupId, DateTime checkInDate, DateTime checkOutDate, DateTime now)
        {
            var roomActLst = rooms.Select(it =>
                new RoomActivated
                {
                    _id = Guid.NewGuid().ToString(),
                    GroupId = groupId,
                    RoomNo = it.RoomNo,
                    ArrivalDate = checkInDate,
                    Departure = checkOutDate,
                    Setting = it.Setting,
                    Status = "จอง",
                    Active = true,
                    CreationDateTime = now,
                    LastUpdate = now,
                }).ToList();

            await CollectionRoomActivated.InsertManyAsync(roomActLst);
        }

        private async Task DeleteRoomAct(string groupId, DateTime now)
        {
            var defDelete = Builders<RoomActivated>.Update
                .Set(r => r.Status, "ยกเลิก")
                .Set(r => r.Active, false)
                .Set(r => r.LastUpdate, now);

            await CollectionRoomActivated.UpdateManyAsync(r => r.GroupId == groupId, defDelete);
        }

        private async Task DeleteRoomAct(string groupId, DateTime now, IEnumerable<string> rooms)
        {
            var defDelete = Builders<RoomActivated>.Update
                .Set(r => r.Status, "ยกเลิก")
                .Set(r => r.Active, false)
                .Set(r => r.LastUpdate, now);

            await CollectionRoomActivated.UpdateManyAsync(r => r.GroupId == groupId && rooms.Contains(r.RoomNo), defDelete);
        }
    }
}