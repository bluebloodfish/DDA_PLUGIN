using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.Utility
{
    public class OrderProcessResult
    {
        public ProcessStatusInfo Result { get; set; }
        public string PosOrderNo { get; set; }
        public long TTOrderId { get; set; }
        public string ErrorId { get; set; }
    }







}
