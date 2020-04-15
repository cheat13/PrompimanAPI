namespace PrompimanAPI.Models
{
    public class Room
    {
        public string _id { get; set; }
        public RoomType RoomType { get; set; }
        public BedType BedType { get; set; }
        public int Rate { get; set; }
        public string Status { get; set; } // ว่าง, แจ้งซ่อม, ห้องพักผู้บริหาร, ขายแล้ว
    }

    public enum RoomType
    {
        Standard = 1,
        Superior = 2,
        Deluxe = 3,
        GrandDeluxe = 4
    }

    public enum BedType
    {
        Single = 1,
        Twin = 2
    }
}