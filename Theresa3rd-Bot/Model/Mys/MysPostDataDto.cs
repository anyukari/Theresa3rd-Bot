﻿using System.Collections.Generic;

namespace Theresa3rd_Bot.Model.Mys
{
    public class MysPostDataDto
    {
        public List<MysPostListDto> list { get; set; }
    }

    public class MysPostListDto
    {
        public MysPostInfoDto post { get; set; }

        public MysPostUserDto user { get; set; }
    }

    public class MysPostInfoDto
    {
        public string content { get; set; }
        public int created_at { get; set; }
        public List<string> images { get; set; }
        public string subject { get; set; }
        public string uid { get; set; }
        public string post_id { get; set; }
    }

    public class MysPostUserDto
    {
        public string avatar { get; set; }
        public string avatar_url { get; set; }
        public string nickname { get; set; }
        public string pendant { get; set; }
        public string uid { get; set; }
    }

}
