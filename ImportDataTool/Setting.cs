﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportDataTool
{
    public class Setting
    {
        public string? Version { get; set; }
        public string? Type { get; set; }
        public string? DateStart { get; set; }
        public string? DateEnd { get; set; }
        public string? Date { get; set; }
        public MySqlConfig? MySqlConfig { get; set; }
    }
}
