﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Theresa3rd_Bot.Model.PO;

namespace Theresa3rd_Bot.Model.Subscribe
{
    public class MysSubscribe
    {
        public SubscribeRecordPO SubscribeRecord { get; set; }

        public FileInfo CoverFileInfo { get; set; }

        public FileInfo PreviewFileInfo { get; set; }
    }
}
