using System;
using System.Collections.Generic;

namespace PrompimanAPI.Models
{
    public class MemberListData
    {
        public IEnumerable<Member> Members { get; set; }
        public int Page { get; set; }
        public int Count { get; set; }
    }
}