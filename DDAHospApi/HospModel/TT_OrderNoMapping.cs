using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.HospModel
{
    [Table("TT_OrderNoMapping")]
    public class TT_OrderNoMapping
    {
        public Int64 Id { get; set; }
        public string OrderNo { get; set; }
        public int TTId { get; set; }
        public string PlatOrderNo { get; set; }
        public string PlatName { get; set; }


    }
}
