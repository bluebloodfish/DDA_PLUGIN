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
        public string PlatOrderNo { get; set; }
        public long TTOrderId { get; set; }
        public int OrderType { get; set; } //0: Hosp Simple Order; 1: Hosp code matching order;  2: Tyro Order  
        public string ErrorId { get; set; }
    }







}
