using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
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

        [HttpGet("{idCard}")]
        public async Task<ActionResult<Member>> GetByIdCard(string idCard)
        {
            return await CollectionMember.Find(m => m.IdCard == idCard).FirstOrDefaultAsync();
        }

        [HttpPost]
        public async Task<Response> Create([FromBody] Member member)
        {
            var isOldMember = await CollectionMember.Find(m => (!string.IsNullOrEmpty(m.IdCard) && m.IdCard == member.IdCard)
                || (!string.IsNullOrEmpty(m.PassportNo) && m.PassportNo == member.PassportNo)).AnyAsync();

            if (isOldMember)
            {
                return new Response
                {
                    IsSuccess = false,
                    ErrorMessage = "เป็นสมาชิกอยู่แล้ว",
                };
            }
            else
            {
                var now = DateTime.Now;
                member._id = now.Ticks.ToString();
                member.CreationDateTime = now;
                member.LastUpdate = now;

                member.Nationality = member.Nationality ?? "ไทย";
                member.Job = member.Job ?? "รับจ้าง";

                await CollectionMember.InsertOneAsync(member);

                return new Response
                {
                    IsSuccess = true,
                };
            }
        }

        [HttpPost]
        public async Task<Response> CreateTh([FromBody] ThMember member)
        {
            var isOldMember = await CollectionMember.Find(m => m.IdCard == member.IdCard).AnyAsync();
            if (isOldMember)
            {
                return new Response
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
                    Nationality = "ไทย",
                    CreationDateTime = now,
                    LastUpdate = now,
                };

                await CollectionMember.InsertOneAsync(newMember);

                return new Response
                {
                    IsSuccess = true,
                };
            }
        }

        [HttpPost]
        public async Task<Response> CreateEn([FromBody] EnMember member)
        {
            var isOldMember = await CollectionMember.Find(m => m.PassportNo == member.PassportNo).AnyAsync();
            if (isOldMember)
            {
                return new Response
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
                    Job = "รับจ้าง",
                    Nationality = member.Nationality,
                    CreationDateTime = now,
                    LastUpdate = now,
                };

                await CollectionMember.InsertOneAsync(newMember);

                return new Response
                {
                    IsSuccess = true,
                };
            }
        }

        [HttpPut("{id}")]
        public async Task<Response> Update(string id, [FromBody] Member member)
        {
            member.LastUpdate = DateTime.Now;

            await CollectionMember.ReplaceOneAsync(m => m._id == id, member);

            return new Response
            {
                IsSuccess = true,
            };
        }

        [HttpPost]
        public async Task<PhotoResponse> UploadPhoto([FromBody] PhotoRequest request)
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=saladpukstorage;AccountKey=V84hggJN/t56SYwQHoMDUt5kFD2bOOtUdxwK5ndMdRCyBZ4kAo8WLz7pU/H09zfrdS+SmmC8aYJsrWwoYubm4Q==;EndpointSuffix=core.windows.net";

            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(connectionString, out storageAccount))
            {
                var url = "";
                var containerName = "photos"; // ตั้งชื่อได้เอง
                var blobName = $"{request.IdCard}.png";

                var cloudBlobClient = storageAccount.CreateCloudBlobClient();
                var cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
                await cloudBlobContainer.CreateIfNotExistsAsync();

                var permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };
                await cloudBlobContainer.SetPermissionsAsync(permissions);

                var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
                cloudBlockBlob.Properties.ContentType = "image/png";

                await cloudBlockBlob.UploadFromByteArrayAsync(request.PhotoRaw, 0, request.PhotoRaw.Length);

                return new PhotoResponse
                {
                    IsSuccess = true,
                    Path = $"{url}/{containerName}/{blobName}",
                };
            }
            else
            {
                return new PhotoResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "The connection string isn't valid",
                };
            }
        }
    }
}