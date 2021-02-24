using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.WebApi.Model.MT_Menu
{
    public class MT_CategoryResponse
    {
        public int Total_Rows { get; set; }
        public int Total_Pages { get; set; }
        public List<MT_Category> Categories { get; set; }

    }

    public class MT_Category
    {
        public string Code { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public bool Enable { get; set; }
    }

}
