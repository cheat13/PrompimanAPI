using System;
using System.Collections.Generic;

namespace PrompimanAPI.Models
{
    public class RoomActivated
    {
        public string _id { get; set; }
        public string GroupId { get; set; }
        public string RoomNo { get; set; }
        public RoomType RoomType { get; set; }
        public BedType BedType { get; set; }
        public int Rate { get; set; }
        public int Discount { get; set; }
        public DateTime ArrivalDate { get; set; }
        public DateTime Departure { get; set; }
        public IEnumerable<Expense> ExpenseList { get; set; }
        public int TotalCost { get; set; }
        public int Paid { get; set; }
        public int Remaining { get; set; }
        public string Status { get; set; } // จอง, เข้าพัก, คืนห้อง, ออก, ยกเลิก
        public string Note { get; set; }
        public bool Active { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public class Expense
    {
        public bool IsPaid { get; set; }
        public bool IsSelected { get; set; }
        public string Name { get; set; }
        public IEnumerable<Detail> Details { get; set; }
        public int TotalCost { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public class Detail
    {
        public string Name { get; set; }
        public int Cost { get; set; }
        public int Count { get; set; }
    }
}