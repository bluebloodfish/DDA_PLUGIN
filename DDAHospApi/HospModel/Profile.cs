using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.HospModel
{
    [Table("Profile")]
    public class Profile
    {
        public string CompanyName { get; set; }

        public bool ManuallyPrintJobList { get; set; }
        public bool AutoPrintJobList { get; set; }
    }
}
