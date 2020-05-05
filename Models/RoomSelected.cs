namespace PrompimanAPI.Models
{
    public class RoomSelected
    {
        public string RoomNo { get; set; }
        public SettingRoom Setting { get; set; }
    }

    public class SettingRoom
    {
        public bool HaveBreakfast { get; set; }
        public bool HaveAddBreakfast { get; set; }
        public int AddBreakfastCount { get; set; }
        public bool HaveExtraBed { get; set; }
        public int ExtraBedCount { get; set; }
        public int Discount { get; set; }
    }
}