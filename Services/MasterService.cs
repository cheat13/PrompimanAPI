using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using PrompimanAPI.Dac;
using PrompimanAPI.Models;

namespace PrompimanAPI.Services
{
    public class MasterService : IMasterService
    {
        private readonly IMasterDac masterDac;
        private readonly IRoomActivatedDac roomActivatedDac;

        public MasterService(
            IMasterDac masterDac,
            IRoomActivatedDac roomActivatedDac)
        {
            this.masterDac = masterDac;
            this.roomActivatedDac = roomActivatedDac;
        }

        public async Task<DataPaging<Master>> GetDataPaging(int page, int size, string word, bool active = true)
        {
            var filter = CreateFilter(word, active);
            var start = Math.Max(0, page - 1) * size;

            var masters = await masterDac.Gets(filter, start, size);
            var count = await masterDac.Count(filter);

            return new DataPaging<Master>
            {
                DataList = masters,
                Page = page,
                Count = (int)count,
            };
        }

        private FilterDefinition<Master> CreateFilter(string word, bool active)
        {
            var fb = Builders<Master>.Filter;
            FilterDefinition<Master> carryFilter = fb.Where(m => m.Active == active);

            if (!string.IsNullOrEmpty(word))
            {
                word = word.ToLower();
                var filter = fb.Where(m => m.Name.ToLower().Contains(word)
                    || m.GroupName.ToLower().Contains(word)
                    || m.Telephone.Contains(word)
                    || m.Rooms.Any(room => room.RoomNo == word));
                carryFilter = filter & carryFilter;
            }

            return carryFilter;
        }

        public async Task<DataPaging<Master>> GetAllCheckOut(int page, int size, string word, bool haveRemaining)
        {
            var masters = new List<Master>();
            
            var filter = CreateFilterRemaining(haveRemaining);
            var allmaster = await masterDac.Gets(filter);

            foreach (var master in allmaster)
            {
                var anyRoomActive = await roomActivatedDac.Any(r => r.GroupId == master._id && r.Active == true);
                if (anyRoomActive == false) masters.Add(master);
            }

            var dataLst = Searching(masters, word);
            var count = masters.Count();

            return new DataPaging<Master>
            {
                DataList = dataLst,
                Page = page,
                Count = count
            };
        }

        private FilterDefinition<Master> CreateFilterRemaining(bool haveRemaining)
        {
            var fb = Builders<Master>.Filter;
            FilterDefinition<Master> carryFilter = fb.Where(m => m.Active == true);

            var filter = (haveRemaining == true)
                ? fb.Where(m => m.Remaining != 0)
                : fb.Where(m => m.Remaining == 0);

            carryFilter = filter & carryFilter;

            return carryFilter;
        }

        private IEnumerable<Master> Searching(IEnumerable<Master> masters, string word)
        {
            return masters.Where(m => m.Name.ToLower().Contains(word)
               || m.GroupName.ToLower().Contains(word)
               || m.Telephone.Contains(word)
               || m.Rooms.Any(room => room.RoomNo == word));
        }
    }
}