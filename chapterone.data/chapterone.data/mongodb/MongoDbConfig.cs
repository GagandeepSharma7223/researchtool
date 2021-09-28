using chapterone.data.interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace chapterone.data.mongodb
{
    public class MongoDbConfig : IDatabaseConfig
    {
        public string Endpoint { get; set; }
        public string Database { get; set; }

        public MongoDbConfig(string endpoint, string database)
        {
            Endpoint = endpoint;
            Database = database;
        }
    }
}
