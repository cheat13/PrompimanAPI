using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Services
{
    public interface IDbService
    {
        IMongoCollection<Member> CollectionMember { get; set; }
        IMongoCollection<Reservation> CollectionReservation { get; set; }
        IMongoCollection<RoomActivated> CollectionRoomActivated { get; set; }
        IMongoCollection<Room> CollectionRoom { get; set; }
    }
}