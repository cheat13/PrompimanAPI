using System;
using System.Collections.Generic;

namespace PrompimanAPI.Models
{
    public class Reservation
    {
        public string _id { get; set; }
        public string Name { get; set; }
        public string Telephone { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public IEnumerable<RoomSelected> Rooms { get; set; }
        public int Reserve { get; set; }
        public bool IsConfirm { get; set; }
        public bool Active { get; set; }
        public string Note { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}