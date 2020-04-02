using System;

namespace PrompimanAPI.Models
{
    public class EnMember
    {
        public string PassportNo { get; set; }
        public string En_Prefix { get; }
        public string En_Firstname { get; }
        public string En_Lastname { get; }
        public string Sex { get; }
        public DateTime Birthday { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Telephone { get; set; }
        public string Nationality { get; set; }
    }
}