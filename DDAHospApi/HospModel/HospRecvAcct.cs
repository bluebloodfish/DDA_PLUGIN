using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.HospModel
{
    [Table("RecvAcct")]
    public class HospRecvAcct
    {
        public string OrderNo { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime? AccountDate { get; set; }
        public double PaidAmount { get; set; }
        public string Payby { get; set; }
        public int IDNo { get; set; }
        public string OpName { get; set; }
        public string MachineID { get; set; }


        public bool Transfer { get; set; }
        public int DepositID { get; set; }
        public double GiftCardBalance { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime? GiftCardExpireDate { get; set; }
        public string Notes { get; set; }


        public HospRecvAcct Clone() {
            return new HospRecvAcct {
                OrderNo = this.OrderNo,
                AccountDate = DateTime.Now,
                PaidAmount = this.PaidAmount,
                Payby = this.Payby,
                IDNo = this.IDNo,
                OpName = this.OpName,
                MachineID = this.MachineID,
                Transfer = true,
                DepositID = 0,
                GiftCardBalance = 0,
                GiftCardExpireDate = new DateTime(1900, 1, 1),
                Notes = ""

            };
        }
    }
}

