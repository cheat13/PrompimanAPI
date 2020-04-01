using System;

namespace PrompimanAPI.Models
{
    public class ThMember
    {
        public string IDCard { get; set; }
        public string Th_Prefix { get; }
        public string Th_Firstname { get; }
        public string Th_Lastname { get; }
        public Sex Sex { get; }
        public DateTime Birthday { get; set; }
        public string Address { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Telephone { get; set; }
        public string Job { get; set; }
    }
}