using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.HospModel
{
    [Table("TT_ApiSetting")]
    public class TT_ApiSetting
    {

        public int Id { get; set; }

        public string HttpBaseUrl { get; set; }

        public int OnlineOrderStartYear { get; set; }
    }
}
