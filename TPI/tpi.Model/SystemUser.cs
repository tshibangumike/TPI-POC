using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tpi.Model
{
    public class SystemUser
    {
        public Guid Id { get; set; }
        public string Fullname { get; set; }
        public string[] InspectorSkills { get; set; }
    }
}