using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Services
{
    public class MemberService : IMemberService
    {
        public FilterDefinition<Member> CreateFilter(string word)
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