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
    public class CheckInController : ControllerBase
    {
        private readonly IMasterDac masterDac;
        private readonly IRoomDac roomDac;
        private readonly IRoomActivatedDac roomActivatedDac;
        private readonly DateTime now;
        private readonly IMasterService masterService;
        private readonly IRoomActService roomActService;

        public CheckInController(
            IMasterDac masterDac,
            IRoomDac roomDac,
            IRoomActivatedDac roomActivatedDac,
            IMasterService masterService,
            IRoomActService roomActService)
        {
            this.masterDac = masterDac;
            this.roomDac = roomDac;
            this.roomActivatedDac = roomActivatedDac;
            this.now = DateTime.Now;
            this.masterService = masterService;
            this.roomActService = roomActService;
        }

        [HttpGet("{memberId}/{groupName}")]
        public async Task<GroupNameChecked> IsAlready(string memberId, string groupName)
        {
            var master = await masterDac.Get(x => x.GroupName == groupName && x.Active == true);

            return (master == null)
                ? GroupNameChecked.NotHave
                : (master._id == memberId)
                    ? GroupNameChecked.OldMaster
                    : GroupNameChecked.NewMaster;
        }

        [HttpPut]
        public async Task<IEnumerable<RoomActivated>> GetRoomActLst([FromBody] Master master)
        {
            var req = new CreateRoomActRequest
            {
                GroupId = master._id,
                RoomSltLst = master.Rooms,
                CheckInDate = master.CheckInDate,
                CheckOutDate = master.CheckOutDate,
            };
            var status = "เข้าพัก";

            var roomActLst = await roomActService.Create(req, status, now);

            return roomActService.SetSelected(roomActLst);
        }

        [HttpPut]
        public async Task<Response> Create([FromBody] MasterDetail req)
        {
            var master = req.Master;
            var oldMaster = await masterDac.Get(x => x.MemberId == master.MemberId && x.GroupName == master.GroupName && x.Active == true);

            if (string.IsNullOrEmpty(master._id)) master._id = oldMaster?._id ?? now.Ticks.ToString();

            var roomActLst = req.RoomActLst.Select(it =>
                {
                    var calc = roomActService.CalculateExpense(it.ExpenseList, now);

                    return new RoomActivated
                    {
                        _id = $"{master._id}{it.RoomNo}",
                        GroupId = master._id,
                        RoomNo = it.RoomNo,
                        RoomType = it.RoomType,
                        BedType = it.BedType,
                        Rate = it.Rate,
                        ArrivalDate = master.CheckInDate,
                        Departure = master.CheckOutDate,
                        ExpenseList = calc.ExpenseList,
                        TotalCost = calc.TotalCost,
                        Paid = calc.Paid,
                        Remaining = calc.Remaining,
                        Status = it.Status,
                        Active = it.Active,
                        CreationDateTime = now,
                        LastUpdate = now,
                    };
                });

            var writeModels = roomActLst
                .OrderBy(it => it.RoomNo)
                .Select(it =>
                new ReplaceOneModel<RoomActivated>(Builders<RoomActivated>.Filter.Eq(d => d._id, it._id), it)
                {
                    IsUpsert = true
                });

            await roomActivatedDac.Creates(writeModels);

            master.TotalCost = roomActLst.Sum(room => room.TotalCost);
            master.Paid = roomActLst.Sum(room => room.Paid);
            master.Remaining = roomActLst.Sum(room => room.Remaining);
            if (master.HaveRoomDeposit) master.Deposit = 200 * master.Rooms.Count(); // ห้องละ 200 ???

            if (oldMaster != null)
            {
                if (master.CheckInDate < oldMaster.CheckInDate) oldMaster.CheckInDate = master.CheckInDate;
                if (master.CheckOutDate > oldMaster.CheckOutDate) oldMaster.CheckOutDate = master.CheckOutDate;

                oldMaster.Rooms = oldMaster.Rooms.Concat(master.Rooms).OrderBy(it => it.RoomNo);
                oldMaster.HaveRoomDeposit = oldMaster.HaveRoomDeposit || master.HaveRoomDeposit;
                oldMaster.HaveTaxInvoice = oldMaster.HaveTaxInvoice || master.HaveTaxInvoice;
                oldMaster.Deposit += master.Deposit;
                oldMaster.TotalCost += master.TotalCost;
                oldMaster.Paid += master.Paid;
                oldMaster.Remaining += master.Remaining;
                oldMaster.LastUpdate = now;

                await masterDac.Update(x => x._id == oldMaster._id, oldMaster);
            }
            else
            {
                master.Active = true;
                master.CreationDateTime = now;
                master.LastUpdate = now;

                await masterDac.Create(master);
            }

            return new Response
            {
                IsSuccess = true
            };
        }

        [HttpGet("{page}/{size}")]
        public async Task<ActionResult<DataPaging<Master>>> Get(int page, int size, string word = "")
        {
            return await masterService.GetDataPaging(page, size, word);
        }

        [HttpGet("{page}/{size}/{haveRemaining}")]
        public async Task<DataPaging<Master>> GetAllCheckOut(int page, int size, bool haveRemaining, string word = "")
        {
            return await masterService.GetAllCheckOut(page, size, word, haveRemaining);
        }

        [HttpGet("{page}/{size}")]
        public async Task<DataPaging<Master>> GetHistory(int page, int size, string word = "")
        {
            return await masterService.GetDataPaging(page, size, word, false);
        }

        [HttpGet("{masterId}")]
        public async Task<MasterDetail> GetById(string masterId)
        {
            var master = await masterDac.Get(m => m._id == masterId);
            var roomActLst = await roomActivatedDac.Gets(r => r.GroupId == masterId);

            return new MasterDetail
            {
                Master = master,
                RoomActLst = roomActLst,
            };
        }

        [HttpGet("{masterId}")]
        public async Task<IEnumerable<RoomActivated>> GetRoomActLst(string masterId, string roomId = null)
        {
            var roomActLst = await roomActivatedDac.Gets(x => x.GroupId == masterId);

            return roomActService.SetSelected(roomActLst, roomId);
        }

        [HttpPut]
        public async Task<Response> Update([FromBody] RoomActRequest req)
        {
            var calcLst = new List<CalculateExpense>();

            foreach (var roomAct in req.RoomActLst)
            {
                var calc = roomActService.CalculateExpense(roomAct.ExpenseList, now);
                calcLst.Add(calc);

                var anySelect = roomAct.ExpenseList.Any(ex => ex.IsSelected == true);
                if (anySelect)
                {
                    var defRoom = Builders<RoomActivated>.Update
                        .Set(r => r.ExpenseList, calc.ExpenseList)
                        .Set(r => r.TotalCost, calc.TotalCost)
                        .Set(r => r.Paid, calc.Paid)
                        .Set(r => r.Remaining, calc.Remaining)
                        .Set(r => r.LastUpdate, now);

                    await roomActivatedDac.Update(r => r._id == roomAct._id, defRoom);
                }
            }

            var def = Builders<Master>.Update
                .Set(m => m.TotalCost, calcLst.Sum(it => it.TotalCost))
                .Set(m => m.Paid, calcLst.Sum(it => it.Paid))
                .Set(m => m.Remaining, calcLst.Sum(it => it.Remaining))
                .Set(m => m.LastUpdate, now);

            await masterDac.Update(m => m._id == req.MasterId, def);

            return new Response
            {
                IsSuccess = true
            };
        }
    }
}