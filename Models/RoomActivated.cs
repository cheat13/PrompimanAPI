using System;

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
        public DateTime ArrivalDate { get; set; }
        public DateTime Departure { get; set; }
        public SettingRoom Setting { get; set; }
        public string Status { get; set; } // จอง, เข้าพัก, คืนห้อง, ออก, ยกเลิก
        public bool Active { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public class SettingRoom
    {
        public bool HaveBreakfast { get; set; }
        public bool HaveAddBreakfast { get; set; }
        public int AddBreakfastCount { get; set; }
        public bool HaveExtraBed { get; set; }
        public int ExtraBedCount { get; set; }
        public int Discount { get; set; }
    }

    // public class CostInfo
    // {
    //     public string Name { get; set; }
    //     // public string ExtraId { get; set; }
    //     public int Cost { get; set; }
    //     public bool IsPaid { get; set; }
    // }

    // public class Extra
    // {
    //     public string _id { get; set; }
    //     public string RoomId { get; set; }
    //     public string Detail { get; set; }
    // }
}