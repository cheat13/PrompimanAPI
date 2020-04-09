using System;

namespace PrompimanAPI.Models
{
    public class PhotoResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string Path { get; set; }
    }
}