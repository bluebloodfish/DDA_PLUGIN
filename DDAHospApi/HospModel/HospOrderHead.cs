using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.HospModel
{
    [Table("OrderH")]
    public class HospOrderHead
    {
        [Key]
        public string OrderNo { get; set; }
        public bool Credit { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime OrderDate { get; set; }

        
        public string TableNo { get; set; }
        public string OpName { get; set; }
        public string ServicePerson { get; set; }
        public string MachineID { get; set; }
        public int BillKind { get; set; }
        public bool Delivery { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerTelephone { get; set; }

        public double Amount { get; set; }
        public double GST { get; set; } //
        public double PaidAmount { get; set; }

        public double DollarDiscount { get; set; }
        public string BookingNo { get; set; }
        public bool OrderPrinted { get; set; }
        public double Tips { get; set; }
        public int Persons { get; set; }
        public string InvoiceNo { get; set; }
        public int VIPNo { get; set; }
        public double ServiceCharge { get; set; }
        public double ServiceChargeRate { get; set; }
        public double Surcharge { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DueTime { get; set; }
        public int DiscountKind { get; set; }
        public double OtherCharge { get; set; }
        public double OtherChargeRate { get; set; }
        public bool PriceIncludesGST { get; set; }
        public double CurrentGSTRate { get; set; }
        public bool SplitBill { get; set; }
        public double RedeemPoints { get; set; }
        public string DiscountOperator { get; set; }
        public string MemberID { get; set; }
        public double CurrentPoints { get; set; }
        public bool? PointsUploaded { get; set; }
        public bool AwardEffective { get; set; }
        public string PresetDiscountCode { get; set; }
        public string VoucherID { get; set; }
        public double VoucherAmount { get; set; }
        public double VoucherDiscount { get; set; }
        public double TotalRedeemPoints { get; set; }
        public string SelfOrderMenuGroup { get; set; }

        [NotMapped]
        public string Notes { get; set; }
    }
}
