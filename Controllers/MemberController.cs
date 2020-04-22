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
using PrompimanAPI.Services;

namespace PrompimanAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IDbService dbService;
        private readonly WebConfig webConfig;

        public MemberController(IDbService dbService, WebConfig webConfig)
        {
            this.dbService = dbService;
            this.webConfig = webConfig;
        }

        [HttpGet("{page}/{size}")]
        public async Task<ActionResult<MemberListData>> Get(int page, int size, string word = "")
        {
            var filter = CreateFilter(word);
            var memberQry = dbService.CollectionMember.Find(filter);

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
            FilterDefinition<Member> carryFilter = fb.Where(m => true);

            if (!string.IsNullOrEmpty(word))
            {
                word = word.ToLower();
                var filter = fb.Where(m => m.IdCard.Contains(word)
                    || m.PassportNo.ToLower().Contains(word)
                    || m.Th_Firstname.Contains(word)
                    || m.Th_Lastname.Contains(word)
                    || m.En_Firstname.ToLower().Contains(word)
                    || m.En_Lastname.ToLower().Contains(word)
                    || m.Telephone.Contains(word));
                carryFilter = filter & carryFilter;
            }

            return carryFilter;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetById(string id)
        {
            return await dbService.CollectionMember.Find(m => m._id == id).FirstOrDefaultAsync();
        }

        [HttpGet("{idCard}")]
        public async Task<ActionResult<Member>> GetByIdCard(string idCard)
        {
            return await dbService.CollectionMember.Find(m => m.IdCard == idCard).FirstOrDefaultAsync();
        }

        [HttpPost]
        public async Task<Response> Create([FromBody] Member member)
        {
            var isOldMember = await dbService.CollectionMember.Find(m => (!string.IsNullOrEmpty(m.IdCard) && m.IdCard == member.IdCard)
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

                await dbService.CollectionMember.InsertOneAsync(member);

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

            await dbService.CollectionMember.ReplaceOneAsync(m => m._id == id, member);

            return new Response
            {
                IsSuccess = true,
            };
        }

        [HttpPost]
        public async Task<PhotoResponse> UploadPhoto([FromBody] PhotoRequest request)
        {
            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(webConfig.StorageConnectionString, out storageAccount))
            {
                var baseUrl = webConfig.StorageBaseUrl;
                var containerName = webConfig.StorageContainerName;
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
                    Path = $"{baseUrl}/{containerName}/{blobName}",
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