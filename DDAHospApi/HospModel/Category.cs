using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.HospModel
{
    [Table("Category")]
    public class Category
    {
        [Key]
        public string Code { get; set; }

        [Column("Category")]
        public string Name1 { get; set; }

        [Column("Category1")]
        public string Name2 { get; set; }

        [Column("Category2")]
        public string Name3 { get; set; }

        [Column("Category3")]
        public string Name4 { get; set; }

        public bool Enable { get; set; }

        public bool ShowOnMainMenu { get; set; }
        public bool ShowOnPhoneOrderMenu { get; set; }
        public bool ShowOnPOSMenu { get; set; }
    }
}

