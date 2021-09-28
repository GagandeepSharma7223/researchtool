using System;
using System.Collections.Generic;
using System.Text;

namespace chapterone.data.interfaces
{
    public interface IDatabaseConfig
    {
        string Endpoint { get; set; }
        string Database { get; set; }
    }
}
