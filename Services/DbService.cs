using MongoDB.Driver;
using PrompimanAPI.Models;

namespace PrompimanAPI.Services
{
    public class DbService : IDbService
    {
        public IMongoCollection<Member> CollectionMember { get; set; }
        public IMongoCollection<Reservation> CollectionReservation { get; set; }
        public IMongoCollection<RoomActivated> CollectionRoomActivated { get; set; }
        public IMongoCollection<Room> CollectionRoom { get; set; }
        public IMongoCollection<Master> CollectionMaster { get; set; }

        public DbService(DbConfig dbConfig)
        {
            var client = new MongoClient(dbConfig.MongoDbConnectionString);
            var database = client.GetDatabase(dbConfig.MongoDbName);

            CollectionMember = database.GetCollection<Member>(dbConfig.Member);
            CollectionReservation = database.GetCollection<Reservation>(dbConfig.Reservation);
            CollectionRoomActivated = database.GetCollection<RoomActivated>(dbConfig.RoomActivated);
            CollectionRoom = database.GetCollection<Room>(dbConfig.Room);
            CollectionMaster = database.GetCollection<Master>(dbConfig.Master);
        }
    }
}