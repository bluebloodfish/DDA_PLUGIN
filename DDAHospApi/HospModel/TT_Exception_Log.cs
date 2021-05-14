using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.HospModel
{
    public class TT_Exception_Log
    {
        public Int64 Id { get; set; }
        public DateTime LogDateTime { get; set; }
        public string Source { get; set; }
        public int ErrorNumber { get; set; }
        public int ErrorState { get; set; }

        //public int ErrorSeverity { get; set; }
        public int ErrorLine { get; set; }
        public string ErrorProcedure { get; set; }
        public string ErrorMessage { get; set; }
    }
}
