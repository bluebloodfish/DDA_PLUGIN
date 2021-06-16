using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.HospModel
{
    [Table("TT_OrderProcess_Log")]
    public class TT_OrderProcess_Log
    {
        public Int64 Id { get; set; }
        public DateTime LogDateTime { get; set; }
        public string DDAOrderNo { get; set; }
        public string PlatformOrderNo { get; set; }
        public string TTOrderId { get; set; }
        public int Status { get; set; }
        public string StatusNotes { get; set; }
        public string ErrorId { get; set; }
        public string JsonStr { get; set; }

    }
}
