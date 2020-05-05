using System;
using System.Collections.Generic;

namespace PrompimanAPI.Models
{
    public class MasterDetail
    {
        public Master Master { get; set; }
        public IEnumerable<RoomActivated> RoomActLst { get; set; }
    }
}