﻿using SqlSugar;

namespace Theresa3rd_Bot.Model.PO
{
    public class BasePO
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "主键,自增")]
        public int Id { get; set; }

        public BasePO() { }
    }
}
