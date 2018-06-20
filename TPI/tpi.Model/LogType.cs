using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tpi.Model
{
    public class Log
    {
        public Guid Id { get; set; }
        public string Name { get;set;}
        public string Message { get; set; }
        public int Level { get; set; }
        public string FunctionName { get; set; }
    }

    public enum LogType
    {
        Error = 1,
        Info = 2,
        Warning = 3
    };


}
