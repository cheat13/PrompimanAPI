﻿using System;
using System.Collections.Generic;

namespace PrompimanAPI.Models
{
    public class RoomActRequest
    {
        public string MasterId { get; set; }
        public IEnumerable<RoomActivated> RoomActLst { get; set; }
    }
}