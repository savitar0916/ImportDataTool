using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportDataTool
{
    public class MySqlConfig
    {
        public int? Id { get; set; }
        public string? Server { get; set; }
        public int? Port { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
        public string? Database { get; set; }
        public string? Table { get; set; }
        public List<string>? Columns { get; set; }
    }
}
