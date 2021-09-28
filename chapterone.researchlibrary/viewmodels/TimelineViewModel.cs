using chapterone.data.models;
using NodaTime;
using System.Collections.Generic;

namespace chapterone.web.viewmodels
{
    public class TimelineViewModel
    {
        public int CurrentPage { get; set; }

        public int MaxPages { get; set; }

        public IEnumerable<Message> Entries { get; set; } = new List<Message>();
    }
}
