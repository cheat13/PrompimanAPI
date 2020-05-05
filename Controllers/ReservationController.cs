using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PrompimanAPI.Dac;
using PrompimanAPI.Models;
using PrompimanAPI.Services;

namespace PrompimanAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IRoomDac roomDac;
        private readonly IReservationDac reservationDac;
        private readonly IRoomActivatedDac roomActivatedDac;
        private readonly DateTime now;
        private readonly IRoomActService roomActService;

        public ReservationController(
            IRoomDac roomDac,
            IReservationDac reservationDac,
            IRoomActivatedDac roomActivatedDac,
            IRoomActService roomActService)
        {
            this.roomDac = roomDac;
            this.reservationDac = reservationDac;
            this.roomActivatedDac = roomActivatedDac;
            this.now = DateTime.Now;
            this.roomActService = roomActService;
        }

        [HttpGet]
        public async Task<IEnumerable<Reservation>> Get(string word)
        {
            var filterReservation = CreateFilter(word);
            return await reservationDac.Gets(filterReservation);
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
            return await reservationDac.Get(r => r._id == id);
        }

        [HttpPost]
        public async Task<Response> Create([FromBody] Reservation res)
        {
            // Create Reservation
            res._id = now.Ticks.ToString();
            res.Active = true;
            res.CreationDateTime = now;
            res.LastUpdate = now;

            await reservationDac.Create(res);

            // Create RoomActivated
            var req = new CreateRoomActRequest
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

        private async Task UpsertRoomAct(CreateRoomActRequest req)
        {
            var roomActLst = await roomActService.CreateRoomActLst(req, "จอง", now);

            var writeModels = roomActLst
               .OrderBy(it => it.RoomNo)
               .Select(it =>
               new ReplaceOneModel<RoomActivated>(Builders<RoomActivated>.Filter.Eq(d => d._id, it._id), it)
               {
                   IsUpsert = true
               });

            await roomActivatedDac.Creates(writeModels);
        }

        [HttpPut("{id}")]
        public async Task<Response> Update(string id, int addReserve, [FromBody] Reservation res)
        {
            // Update Reservation
            res.Reserve += addReserve;
            res.LastUpdate = now;

            await reservationDac.Update(r => r._id == id, res);

            // Upsert RoomActivated
            var req = new CreateRoomActRequest
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
            var roomNotActLst = await roomActivatedDac.Gets(filter);

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

            await reservationDac.Update(r => r._id == id, def);

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

            await roomActivatedDac.Updates(filter, def);
        }

        [HttpPut("{id}")]
        public async Task<Response> Confirm(string id)
        {
            var def = Builders<Reservation>.Update
                .Set(it => it.IsConfirm, true)
                .Set(it => it.LastUpdate, now);

            await reservationDac.Update(it => it._id == id, def);

            return new Response
            {
                IsSuccess = true
            };
        }
    }
}