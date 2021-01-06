using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.HospModel
{
    [Table("Version")]
    public class DDAVersion
    {
        public double Version { get; set; }

    }
}
