using MatthiWare.CommandLine.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public class ScriptOptions
    {
        [Required, Name("ip","Ip adress"),Description("The Ip adress that we want to search for")]
        public string IpAdress { get; set; }
        [Required, Name("c", "command location"), Description("The location of the command (.txt)")]
        public string commandLocation { get; set; }
        [Required, Name("o", "output location"), Description("The location of the the user log (.txt)")]
        public string outputLocation { get; set; }

        [Required, Name("t", "time"), Description("How often should it run (m)")]
        public int time { get; set; }
        
    }
}
