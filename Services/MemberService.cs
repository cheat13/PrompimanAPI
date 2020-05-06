using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using PrompimanAPI.Dac;
using PrompimanAPI.Models;

namespace PrompimanAPI.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberDac memberDac;

        public MemberService(IMemberDac memberDac)
        {
            this.memberDac = memberDac;
        }

        public async Task<DataPaging<Member>> GetDataPaging(int page, int size, string word)
        {
            var filter = CreateFilter(word);
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

        private FilterDefinition<Member> CreateFilter(string word)
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
    }
}