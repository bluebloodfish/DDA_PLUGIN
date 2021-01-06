using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.HospModel
{
    public class HospOrder
    {
        public HospOrderHead OrderHead { get; set; }
        public List<HospOrderItem> OrderItems { get; set; }
        public List<HospRecvAcct> RecvAcctList { get; set; }
    }
}
