using System;

namespace PrompimanAPI.Models
{
    public class PhotoRequest
    {
        public string IdCard { get; set; }
        public byte[] PhotoRaw { get; set; }
    }
}