using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace tpi.Model
{
    public class TransactionObject
    {
        public Order Order { get; set; }
        public string SharedPaymentUrl { get; set; }
    }
}
