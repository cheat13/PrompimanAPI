using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrompimanAPI.Dac;
using PrompimanAPI.Models;

namespace PrompimanAPI.Services
{
    public class RoomActService : IRoomActService
    {
        private readonly IRoomDac roomDac;
        public RoomActService(IRoomDac roomDac)
        {
            this.roomDac = roomDac;
        }

        public async Task<IEnumerable<RoomActivated>> CreateRoomActLst(CreateRoomActRequest req, string status, DateTime time)
        {
            var roomNoLst = req.RoomSltLst.Select(r => r.RoomNo).ToList();
            var rooms = await roomDac.Gets(r => roomNoLst.Contains(r._id));

            var roomActLst = req.RoomSltLst.Select(it =>
                {
                    var room = rooms.First(r => r._id == it.RoomNo);
                    var expenseList = CreateExpenseList(it.Setting, room.Rate, time);
                    var totalCost = expenseList.Sum(expense => expense.TotalCost);
                    var paid = expenseList.Where(expense => expense.IsPaid == true).Sum(expense => expense.TotalCost);
                    var remaining = expenseList.Where(expense => expense.IsPaid == false).Sum(expense => expense.TotalCost);

                    return new RoomActivated
                    {
                        _id = $"{req.GroupId}{room._id}",
                        GroupId = req.GroupId,
                        RoomNo = room._id,
                        RoomType = room.RoomType,
                        BedType = room.BedType,
                        Rate = room.Rate,
                        Discount = it.Setting.Discount,
                        ArrivalDate = req.CheckInDate,
                        Departure = req.CheckOutDate,
                        ExpenseList = expenseList,
                        TotalCost = totalCost,
                        Paid = paid,
                        Remaining = remaining,
                        Status = status,
                        Active = true,
                        CreationDateTime = time,
                        LastUpdate = time,
                    };
                }).ToList();

            return roomActLst;
        }

        private IEnumerable<Expense> CreateExpenseList(SettingRoom setting, int rate, DateTime time)
        {
            var roomRate = new Expense
            {
                Name = "ค่าห้อง",
                TotalCost = rate - setting.Discount, // ค่าห้อง - ส่วนลด
                CreationDateTime = time
            };

            var expenseList = new List<Expense> { roomRate };

            if (setting.HaveBreakfast == true)
            {
                var breakfast = new Expense
                {
                    Name = "อาหารเช้า",
                    TotalCost = 200,
                    CreationDateTime = time
                };
                expenseList.Add(breakfast);
            }

            if (setting.HaveAddBreakfast == true)
            {
                var addBreakfast = new Expense
                {
                    Name = "เพิ่มอาหารเช้า",
                    Details = new List<Detail> {
                        new Detail {
                            Name = "อาหารเช้า",
                            Cost = 120,
                            Count = setting.AddBreakfastCount,
                        }},
                    TotalCost = 120 * setting.AddBreakfastCount,
                    CreationDateTime = time
                };
                expenseList.Add(addBreakfast);
            }

            if (setting.HaveExtraBed == true)
            {
                var extraBed = new Expense
                {
                    Name = "เสริมเตียง",
                    Details = new List<Detail> {
                        new Detail {
                            Name = "เตียง",
                            Cost = 100,
                            Count = setting.ExtraBedCount,
                        }},
                    TotalCost = 100 * setting.ExtraBedCount,
                    CreationDateTime = time
                };
                expenseList.Add(extraBed);
            }

            return expenseList;
        }
    }
}