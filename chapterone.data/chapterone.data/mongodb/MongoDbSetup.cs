using MongoDb.Bson.NodaTime;
using MongoDB.Bson.Serialization;

namespace chapterone.data.mongodb
{
    public static class MongoDbSetup
    {
        public static void Setup()
        {
            BsonSerializer.RegisterSerializer(new ZonedDateTimeSerializer());
            //NodaTimeSerializers.Register();
        }
    }
}
