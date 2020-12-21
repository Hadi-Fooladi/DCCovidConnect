using System;
using System.Collections.Generic;
using System.Text;

namespace DCCovidConnect.Models
{
    public class InstagramItem
    {
        public string FullName { get; set; }
        public string ProfileImage { get; set; }
        public List<string> Images { get; set; }
        public string Text { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
    }
}
