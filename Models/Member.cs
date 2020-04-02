using System;

namespace PrompimanAPI.Models
{
    public class Member
    {
        public string _id { get; set; }
        public string IDCard { get; set; }
        public string PassportNo { get; set; }
        public string Th_Prefix { get; set; }
        public string Th_Firstname { get; set; }
        public string Th_Lastname { get; set; }
        public string En_Prefix { get; set; }
        public string En_Firstname { get; set; }
        public string En_Lastname { get; set; }
        public string Sex { get; set; }
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