using System;

namespace PrompimanAPI.Models
{
    public class Member
    {
        public string _id { get; set; }
        public string IDCard { get; set; }
        public string PassportNo { get; set; }
        public string Th_Prefix { get; }
        public string Th_Firstname { get; }
        public string Th_Lastname { get; }
        public string En_Prefix { get; }
        public string En_Firstname { get; }
        public string En_Lastname { get; }
        public Sex Sex { get; }
        public DateTime Birthday { get; set; }
        public string Address { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Telephone { get; set; }
        public string Job { get; set; }
        public string Nationality { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}