using System;
using System.Collections.Generic;

namespace PrompimanAPI.Models
{
    public class Master
    {
        public string _id { get; set; }
        public string MemberId { get; set; }
        public string Name { get; set; }
        public string Telephone { get; set; }
        public string GroupName { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public IEnumerable<RoomSelected> Rooms { get; set; }
        public int Reserve { get; set; }
        public bool HaveRoomDeposit { get; set; }
        public bool HaveTaxInvoice { get; set; }
        public int Deposit { get; set; }
        public int TotalCost { get; set; }
        public int Paid { get; set; }
        public int Remaining { get; set; }
        public bool Active { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}