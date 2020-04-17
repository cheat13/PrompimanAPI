using System;
using System.Collections.Generic;

namespace PrompimanAPI.Models
{
    public class RoomActRequest
    {
        public string GroupId { get; set; }
        public IEnumerable<RoomSelected> RoomSltLst { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public DateTime DateTimeNow { get; set; }
    }
}