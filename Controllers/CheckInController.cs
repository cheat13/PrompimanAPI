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
        private readonly IRoomActService roomActService;

        public CheckInController(
            IMasterDac masterDac,
            IRoomDac roomDac,
            IRoomActivatedDac roomActivatedDac,
            IRoomActService roomActService)
        {
            this.masterDac = masterDac;
            this.roomDac = roomDac;
            this.roomActivatedDac = roomActivatedDac;
            this.now = DateTime.Now;
            this.roomActService = roomActService;
        }

        [HttpGet("{memberId}/{groupName}")]
        public async Task<bool> IsAlready(string memberId, string groupName)
        {
            return await masterDac.Any(x => x.MemberId == memberId && x.GroupName == groupName && x.Active == true);
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

            var roomActLst = await roomActService.CreateRoomActLst(req, status, now);

            return SetSelected(roomActLst);
        }

        private IEnumerable<RoomActivated> SetSelected(IEnumerable<RoomActivated> roomActLst, string roomId = null)
        {
            var qry = roomActLst.ToList();

            qry.ForEach(it =>
            {
                if (string.IsNullOrEmpty(roomId) || it._id == roomId)
                {
                    it.ExpenseList.ToList().ForEach(i => i.IsSelected = true);
                }
            });

            return qry;
        }

        [HttpPut]
        public async Task<Response> Create([FromBody] MasterDetail req)
        {
            var master = req.Master;
            var oldMaster = await masterDac.Get(x => x.MemberId == master.MemberId && x.GroupName == master.GroupName && x.Active == true);

            if (string.IsNullOrEmpty(master._id)) master._id = oldMaster?._id ?? now.Ticks.ToString();

            var roomActLst = req.RoomActLst.Select(it =>
                {
                    var calc = CalculateExpense(it.ExpenseList);

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

        private CalculateExpense CalculateExpense(IEnumerable<Expense> expenseList)
        {
            var qry = expenseList.ToList();

            qry.ForEach(expense =>
            {
                if (expense.IsSelected == true)
                {
                    expense.IsPaid = true;
                    expense.IsSelected = false;
                }
                expense.CreationDateTime = now;
            });

            var totalCost = qry.Sum(expense => expense.TotalCost);
            var paid = qry.Where(expense => expense.IsPaid == true).Sum(expense => expense.TotalCost);
            var remaining = qry.Where(expense => expense.IsPaid == false).Sum(expense => expense.TotalCost);

            return new CalculateExpense
            {
                ExpenseList = qry,
                TotalCost = totalCost,
                Paid = paid,
                Remaining = remaining,
            };
        }

        [HttpGet("{page}/{size}")]
        public async Task<ActionResult<DataPaging<Master>>> Get(int page, int size, string word = "")
        {
            var filter = CreateFilter(word);

            var count = await masterDac.Count(filter);
            var start = Math.Max(0, page - 1) * size;
            var masters = await masterDac.Gets(filter, start, size);

            return new DataPaging<Master>
            {
                DataList = masters,
                Page = page,
                Count = (int)count,
            };
        }

        private static FilterDefinition<Master> CreateFilter(string word)
        {
            var fb = Builders<Master>.Filter;
            FilterDefinition<Master> carryFilter = fb.Where(m => m.Active == true);

            if (!string.IsNullOrEmpty(word))
            {
                word = word.ToLower();
                var filter = fb.Where(m => m.Name.ToLower().Contains(word)
                    || m.GroupName.ToLower().Contains(word)
                    || m.Telephone.Contains(word)
                    || m.Rooms.Any(room => room.RoomNo == word));
                carryFilter = filter & carryFilter;
            }

            return carryFilter;
        }

        [HttpGet("{masterId}")]
        public async Task<ActionResult<MasterDetail>> GetById(string masterId)
        {
            var master = await masterDac.Get(m => m._id == masterId);
            var roomActLst = await roomActivatedDac.Gets(r => r.GroupId == masterId);

            return new MasterDetail
            {
                Master = master,
                RoomActLst = roomActLst,
            };
        }

        [HttpGet("{masterId}/{roomId}")]
        public async Task<IEnumerable<RoomActivated>> GetRoomActLst(string masterId, string roomId)
        {
            var roomActLst = await roomActivatedDac.Gets(x => x.GroupId == masterId);

            return SetSelected(roomActLst, roomId);
        }

        [HttpPut]
        public async Task<Response> Update([FromBody] RoomActRequest req)
        {
            var calcLst = new List<CalculateExpense>();

            foreach (var roomAct in req.RoomActLst)
            {
                var calc = CalculateExpense(roomAct.ExpenseList);
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