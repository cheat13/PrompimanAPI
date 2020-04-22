using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PrompimanAPI.Models;
using PrompimanAPI.Services;

namespace PrompimanAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IDbService dbService;
        private readonly DateTime now;

        public ReservationController(IDbService dbService)
        {
            this.dbService = dbService;
            this.now = DateTime.Now;
        }

        [HttpGet]
        public async Task<IEnumerable<Reservation>> Get(string word)
        {
            var filter = CreateFilter(word);
            return await dbService.CollectionReservation.Find(filter).SortBy(x => x.CheckInDate).ToListAsync();
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
            return await dbService.CollectionReservation.Find(r => r._id == id).FirstOrDefaultAsync();
        }

        [HttpPost]
        public async Task<Response> Create([FromBody] Reservation res)
        {
            // Create Reservation
            res._id = now.Ticks.ToString();
            res.CreationDateTime = now;
            res.LastUpdate = now;
            res.IsConfirm = false;
            res.Active = true;

            await dbService.CollectionReservation.InsertOneAsync(res);

            // Create RoomActivated
            var req = new RoomActRequest
            {
                GroupId = res._id,
                RoomSltLst = res.Rooms,
                CheckInDate = res.CheckInDate,
                CheckOutDate = res.CheckOutDate,
            };
            await UpsertRoomAct(req);

            return new Response
            {
                IsSuccess = true
            };
        }

        private async Task UpsertRoomAct(RoomActRequest req)
        {
            var roomNoLst = req.RoomSltLst.Select(r => r.RoomNo).ToList();
            var rooms = await dbService.CollectionRoom.Find(r => roomNoLst.Contains(r._id)).ToListAsync();

            var roomActLst = req.RoomSltLst.Select(it =>
            {
                var room = rooms.First(r => r._id == it.RoomNo);

                return new RoomActivated
                {
                    _id = $"{req.GroupId}{room._id}",
                    GroupId = req.GroupId,
                    RoomNo = room._id,
                    RoomType = room.RoomType,
                    BedType = room.BedType,
                    Rate = room.Rate,
                    ArrivalDate = req.CheckInDate,
                    Departure = req.CheckOutDate,
                    Setting = it.Setting,
                    Status = "จอง",
                    Active = true,
                    CreationDateTime = now,
                    LastUpdate = now,
                };
            }).ToList();

            var writeModels = roomActLst
               .OrderBy(it => it.RoomNo)
               .Select(it =>
               new ReplaceOneModel<RoomActivated>(Builders<RoomActivated>.Filter.Eq(d => d._id, it._id), it)
               {
                   IsUpsert = true
               });

            await dbService.CollectionRoomActivated.BulkWriteAsync(writeModels);
        }

        [HttpPut("{id}")]
        public async Task<Response> Update(string id, int addReserve, [FromBody] Reservation res)
        {
            // Update Reservation
            var defUpdateRes = Builders<Reservation>.Update
                .Set(r => r.Name, res.Name)
                .Set(r => r.Telephone, res.Telephone)
                .Set(r => r.CheckInDate, res.CheckInDate)
                .Set(r => r.CheckOutDate, res.CheckOutDate)
                .Set(r => r.Rooms, res.Rooms)
                .Set(r => r.Reserve, res.Reserve + addReserve)
                .Set(r => r.LastUpdate, now);

            await dbService.CollectionReservation.UpdateOneAsync(r => r._id == id, defUpdateRes);

            // Upsert RoomActivated
            var req = new RoomActRequest
            {
                GroupId = id,
                RoomSltLst = res.Rooms,
                CheckInDate = res.CheckInDate,
                CheckOutDate = res.CheckOutDate,
            };
            await UpsertRoomAct(req);

            // Delete RoomActivated
            var roomNoLst = res.Rooms.Select(r => r.RoomNo).ToList();
            var filter = Builders<RoomActivated>.Filter.Where(x => x.GroupId == id && x.Active == true && !roomNoLst.Contains(x.RoomNo));
            var roomNotActLst = await dbService.CollectionRoomActivated.Find(filter).ToListAsync();

            if (roomNotActLst.Any())
            {
                var roomActIdLst = roomNotActLst.Select(it => it._id).ToList();
                var filterDel = Builders<RoomActivated>.Filter.Where(r => roomActIdLst.Contains(r._id));
                await DeleteRoomAct(filterDel);
            };

            return new Response
            {
                IsSuccess = true,
            };
        }

        [HttpPut("{id}")]
        public async Task<Response> Delete(string id, string note)
        {
            // Delete Reservation
            var def = Builders<Reservation>.Update
                .Set(r => r.Active, false)
                .Set(r => r.Note, note)
                .Set(r => r.LastUpdate, now);

            await dbService.CollectionReservation.UpdateOneAsync(r => r._id == id, def);

            // Delete RoomActivated
            var filter = Builders<RoomActivated>.Filter.Where(r => r.GroupId == id && r.Active == true);
            await DeleteRoomAct(filter);

            return new Response
            {
                IsSuccess = true,
            };
        }

        private async Task DeleteRoomAct(FilterDefinition<RoomActivated> filter)
        {
            var def = Builders<RoomActivated>.Update
                .Set(r => r.Status, "ยกเลิก")
                .Set(r => r.Active, false)
                .Set(r => r.LastUpdate, now);

            await dbService.CollectionRoomActivated.UpdateManyAsync(filter, def);
        }

        [HttpPut("{id}")]
        public async Task<Response> Confirm(string id)
        {
            var def = Builders<Reservation>.Update
                .Set(it => it.IsConfirm, true)
                .Set(it => it.LastUpdate, now);

            await dbService.CollectionReservation.UpdateOneAsync(it => it._id == id, def);

            return new Response
            {
                IsSuccess = true
            };
        }
    }
}