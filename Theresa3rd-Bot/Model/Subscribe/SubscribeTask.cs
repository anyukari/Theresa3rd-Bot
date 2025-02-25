﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Theresa3rd_Bot.Type;
using Theresa3rd_Bot.Util;

namespace Theresa3rd_Bot.Model.Subscribe
{
    public class SubscribeTask
    {
        public int SubscribeId { get; set; }
        public string SubscribeCode { get; set; }
        public SubscribeType SubscribeType { get; set; }
        public int SubscribeSubType { get; set; }
        public string SubscribeName { get; set; }
        public string SubscribeDescription { get; set; }
        public List<long> GroupIdList { get; set; }

        public SubscribeTask(SubscribeInfo subscribeInfo)
        {
            this.SubscribeId = subscribeInfo.SubscribeId;
            this.SubscribeCode = subscribeInfo.SubscribeCode;
            this.SubscribeType = subscribeInfo.SubscribeType;
            this.SubscribeSubType = subscribeInfo.SubscribeSubType;
            this.SubscribeName = subscribeInfo.SubscribeName;
            this.SubscribeDescription = subscribeInfo.SubscribeDescription;
            this.GroupIdList = new List<long>();
        }


    }
}
