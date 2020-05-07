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

        public async Task<DataPaging<MasterInfo>> GetDataPaging(int page, int size, string word, bool active = true)
        {
            var filter = CreateFilter(word, active);
            var start = Math.Max(0, page - 1) * size;

            var masters = await masterDac.Gets(filter, start, size);
            var masterInfoLst = await GetMasterInfoLst(masters);

            var count = await masterDac.Count(filter);

            return new DataPaging<MasterInfo>
            {
                DataList = masterInfoLst,
                Page = page,
                Count = (int)count,
            };
        }

        private async Task<IEnumerable<MasterInfo>> GetMasterInfoLst(IEnumerable<Master> masters)
        {
            var now = DateTime.Now;

            var masterInfoLst = new List<MasterInfo>();

            foreach (var master in masters)
            {
                var bedNight = await GetBedNight(master._id);
                var daysLeft = (master.CheckOutDate - now).Days;

                var masterInfo = new MasterInfo
                {
                    _id = master._id,
                    GroupName = master.GroupName,
                    BedNight = bedNight,
                    DaysLeft = daysLeft,
                    CheckInDate = master.CheckInDate,
                    CheckOutDate = master.CheckOutDate,
                };

                masterInfoLst.Add(masterInfo);
            }

            return masterInfoLst;
        }

        private async Task<int> GetBedNight(string masterId)
        {
            var rooms = await roomActivatedDac.Gets(r => r.GroupId == masterId);
            return rooms.Sum(r => (r.Departure - r.ArrivalDate).Days);
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

        public async Task<DataPaging<MasterInfo>> GetAllCheckOut(int page, int size, string word, bool haveRemaining)
        {
            var qryMasters = new List<Master>();

            var filter = CreateFilterRemaining(haveRemaining);
            var allmaster = await masterDac.Gets(filter);

            foreach (var master in allmaster)
            {
                var anyRoomActive = await roomActivatedDac.Any(r => r.GroupId == master._id && r.Active == true);
                if (anyRoomActive == false) qryMasters.Add(master);
            }

            var masters = Searching(qryMasters, word);
            var masterInfoLst = await GetMasterInfoLst(masters);

            var count = qryMasters.Count();

            return new DataPaging<MasterInfo>
            {
                DataList = masterInfoLst,
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