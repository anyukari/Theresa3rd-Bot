﻿using System.Collections.Generic;
using Theresa3rd_Bot.Util;

namespace Theresa3rd_Bot.Model.Pixiv
{
    public class PixivBookmarksDto
    {
        public bool error { get; set; }
        public string message { get; set; }
        public PixivBookmarksBody body { get; set; }
    }

    public class PixivBookmarksBody
    {
        public int total { get; set; }
        public List<PixivBookmarksWork> works { get; set; }
    }

    public class PixivBookmarksWork
    {
        public string id { get; set; }
        public int illustType { get; set; }
        public int pageCount { get; set; }
        public string title { get; set; }
        public List<string> tags { get; set; }
        public string userId { get; set; }
        public string userName { get; set; }
        public int xRestrict { get; set; }

        public bool IsImproper() => xRestrict > 1 || (tags != null && tags.IsImproper());

        public bool isR18() => xRestrict > 0 || (tags != null && tags.IsR18());

        public string hasBanTag() => tags?.hasBanTags();

    }


}

