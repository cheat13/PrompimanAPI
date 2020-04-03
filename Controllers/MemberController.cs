using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        public IMongoCollection<Member> CollectionMember { get; set; }

        public MemberController()
        {
            var client = new MongoClient("mongodb://firstclass:Th35F1rstCla55@104.215.253.165/hotel");
            var database = client.GetDatabase("hotel");

            CollectionMember = database.GetCollection<Member>("member");
        }

        [HttpGet("{page}/{size}")]
        public async Task<ActionResult<MemberListData>> Get(int page, int size, string word = "")
        {
            var filter = CreateFilter(word);
            var memberQry = CollectionMember.Find(filter);

            var count = await memberQry.CountDocumentsAsync();
            var start = Math.Max(0, page - 1) * size;
            var members = await memberQry.SortByDescending(it => it._id).Skip(start).Limit(size).ToListAsync();

            return new MemberListData
            {
                Members = members,
                Page = page,
                Count = (int)count,
            };
        }

        private static FilterDefinition<Member> CreateFilter(string word)
        {
            var fb = Builders<Member>.Filter;
            FilterDefinition<Member> filter = fb.Where(m => true);

            if (!string.IsNullOrEmpty(word))
            {
                word = word.ToLower();
                filter = fb.Where(m => m.IdCard.Contains(word)
                    || m.PassportNo.ToLower().Contains(word)
                    || m.Th_Firstname.Contains(word)
                    || m.Th_Lastname.Contains(word)
                    || m.En_Firstname.ToLower().Contains(word)
                    || m.En_Lastname.ToLower().Contains(word)
                    || m.Telephone.Contains(word));
            }

            return filter;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetById(string id)
        {
            return await CollectionMember.Find(m => m._id == id).FirstOrDefaultAsync();
        }

        [HttpPost]
        public async Task<MemberResponse> CreateTh([FromBody] ThMember member)
        {
            var isOldMember = await CollectionMember.Find(m => m.IdCard == member.IdCard).AnyAsync();
            if (isOldMember)
            {
                return new MemberResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "เป็นสมาชิกอยู่แล้ว",
                };
            }
            else
            {
                var now = DateTime.Now;

                var newMember = new Member
                {
                    _id = now.Ticks.ToString(),
                    IdCard = member.IdCard,
                    PassportNo = "",
                    Th_Prefix = member.Th_Prefix,
                    Th_Firstname = member.Th_Firstname,
                    Th_Lastname = member.Th_Lastname,
                    En_Prefix = member.En_Prefix,
                    En_Firstname = member.En_Firstname,
                    En_Lastname = member.En_Lastname,
                    Sex = member.Sex,
                    Birthday = member.Birthday,
                    Address = member.Address,
                    IssueDate = member.IssueDate,
                    ExpiryDate = member.ExpiryDate,
                    Telephone = member.Telephone,
                    Job = member.Job,
                    Nationality = "Thai", // Default ??
                    CreationDateTime = now,
                    LastUpdate = now,
                };

                await CollectionMember.InsertOneAsync(newMember);

                return new MemberResponse
                {
                    IsSuccess = true,
                };
            }
        }

        [HttpPost]
        public async Task<MemberResponse> CreateEn([FromBody] EnMember member)
        {
            var isOldMember = await CollectionMember.Find(m => m.PassportNo == member.PassportNo).AnyAsync();
            if (isOldMember)
            {
                return new MemberResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "เป็นสมาชิกอยู่แล้ว",
                };
            }
            else
            {
                var now = DateTime.Now;

                var newMember = new Member
                {
                    _id = now.Ticks.ToString(),
                    IdCard = "",
                    PassportNo = member.PassportNo,
                    Th_Prefix = "",
                    Th_Firstname = "",
                    Th_Lastname = "",
                    En_Prefix = member.En_Prefix,
                    En_Firstname = member.En_Firstname,
                    En_Lastname = member.En_Lastname,
                    Sex = member.Sex,
                    Birthday = member.Birthday,
                    Address = null,
                    IssueDate = member.IssueDate,
                    ExpiryDate = member.ExpiryDate,
                    Telephone = member.Telephone,
                    Job = null, // Default ??
                    Nationality = member.Nationality,
                    CreationDateTime = now,
                    LastUpdate = now,
                };

                await CollectionMember.InsertOneAsync(newMember);

                return new MemberResponse
                {
                    IsSuccess = true,
                };
            }
        }

        [HttpPut("{id}")]
        public void Update(int id, [FromBody] Member member)
        {
            
        }
    }
}