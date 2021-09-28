using System;
using System.Collections.Generic;
using System.Text;

namespace chapterone.data.mongodb
{
    public class MongoDbException : Exception
    {
        public MongoDbException(string message)
            : base (message)
        {
        }
    }
}
