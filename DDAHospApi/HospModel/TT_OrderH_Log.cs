using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.HospModel
{
    [Table("TT_OrderH_Log")]
    public class TT_OrderH_Log
    {
        public Int64 Id { get; set; }
        public string OrderNo { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        public int OpKind { get; set; }
        public string OpName { get; set; }
        public int Status { get; set; }
        public string TTOrderId { get; set; }
        public int Retried { get; set; }
        public string Message { get; set; }

    }
}
