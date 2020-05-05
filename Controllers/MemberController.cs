using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using MongoDB.Driver;
using PrompimanAPI.Dac;
using PrompimanAPI.Models;
using PrompimanAPI.Services;

namespace PrompimanAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly WebConfig webConfig;
        private readonly IMemberDac memberDac;
        private readonly IMemberService memberService;

        public MemberController(
            WebConfig webConfig,
            IMemberDac memberDac,
            IMemberService memberService)
        {
            this.webConfig = webConfig;
            this.memberDac = memberDac;
            this.memberService = memberService;
        }

        [HttpGet("{page}/{size}")]
        public async Task<ActionResult<DataPaging<Member>>> Get(int page, int size, string word = "")
        {
            var filter = memberService.CreateFilter(word);

            var start = Math.Max(0, page - 1) * size;
            var members = await memberDac.Gets(filter, start, size);
            
            var count = await memberDac.Count(filter);

            return new DataPaging<Member>
            {
                DataList = members,
                Page = page,
                Count = (int)count,
            };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetById(string id)
        {
            return await memberDac.Get(m => m._id == id);
        }

        [HttpGet("{idCard}")]
        public async Task<ActionResult<Member>> GetByIdCard(string idCard)
        {
            return await memberDac.Get(m => m.IdCard == idCard);
        }

        [HttpPost]
        public async Task<Response> Create([FromBody] Member member)
        {
            var filter = Builders<Member>.Filter.Where(m =>
                (!string.IsNullOrEmpty(m.IdCard) && m.IdCard == member.IdCard) ||
                (!string.IsNullOrEmpty(m.PassportNo) && m.PassportNo == member.PassportNo));

            var isOldMember = await memberDac.Any(filter);

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

                if (string.IsNullOrEmpty(member.Nationality)) member.Nationality = "ไทย";
                if (string.IsNullOrEmpty(member.Job)) member.Job = "รับจ้าง";

                await memberDac.Create(member);

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

            await memberDac.Update(m => m._id == id, member);

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