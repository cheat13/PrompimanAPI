using System;
using System.Collections.Generic;

namespace PrompimanAPI.Models
{
    public class DataPaging<T>
    {
        public IEnumerable<T> DataList { get; set; }
        public int Page { get; set; }
        public int Count { get; set; }
    }
}