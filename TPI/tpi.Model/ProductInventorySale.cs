using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tpi.Model
{
    public class ProductInventorySale
    {
        public Guid Id { get; set; }
        public string DownloadUrl { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public Guid PropertyId { get; set; }
        public string PropertyName { get; set; }
        public Guid InspectionId { get; set; }
        public string InspectionName { get; set; }

    }
}
