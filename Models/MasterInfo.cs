using System;
using System.Collections.Generic;

namespace PrompimanAPI.Models
{
    public class MasterInfo
    {
        public string _id { get; set; }
        public string GroupName { get; set; }
        public int BedNight { get; set; }
        public int DaysLeft { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}