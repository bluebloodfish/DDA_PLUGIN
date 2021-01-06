using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.HospModel
{
    [Table("MenuItem")]
    public class MenuItem
    {
        [Key]
        public string ItemCode { get; set; }

        public string Description1 { get; set; }
        public string Description2 { get; set; }
        public string Description3 { get; set; }
        public string Description4 { get; set; }

        public string Category { get; set; }

        public double TaxRate { get; set; }

        [Column("Price")]
        public double Price1 { get; set; }

        [Column("Price1")]
        public double Price2 { get; set; }

        [Column("Price2")]
        public double Price3 { get; set; }

        [Column("Price3")]
        public double Price4 { get; set; }

        [Column("SubDescription")]
        public string SubDescription1 { get; set; }

        [Column("SubDescription1")]
        public string SubDescription2 { get; set; }

        [Column("SubDescription2")]
        public string SubDescription3 { get; set; }

        [Column("SubDescription3")]
        public string SubDescription4 { get; set; }

        public double HappyHourPrice1 { get; set; }
        public double HappyHourPrice2 { get; set; }
        public double HappyHourPrice3 { get; set; }
        public double HappyHourPrice4 { get; set; }

        public int MainPosition { get; set; }

        public bool OnlyShowOnSubMenu { get; set; }

        public string PicturePath { get; set; }

        public bool Instruction { get; set; }


    }
}
