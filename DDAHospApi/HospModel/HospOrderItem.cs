using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.HospModel
{
    [Table("OrderI")]
    public class HospOrderItem
    {
        public string OrderNo { get; set; }
        public double PaidQty { get; set; }
        public double OriginalQty { get; set; }

        public double OriginalPrice { get; set; }
        public double Qty { get; set; }
        public double Price { get; set; }
        public bool PrintFlag { get; set; }
        public bool VoidFlag { get; set; }
        public string OrderOperator { get; set; }
        public Int16 PriceSelect { get; set; }
        public Int16 IDNo { get; set; }
        public string ItemCode { get; set; }
        public double TaxRate { get; set; }
        public Int16 Condition { get; set; }

        public string SpecialOrder { get; set; }
        
        public Int16 Seat { get; set; }
        public double Discount { get; set; }
        public bool SentToKitchen { get; set; }
        public string VoidReason { get; set; }
        public bool CheckListPrinted { get; set; }
        public string PresetDiscountCode { get; set; }
        public bool RedeemItem { get; set; }
        public bool ManuallyEnterWeight { get; set; }

        public HospOrderItem Clone() {
            return new HospOrderItem
            {
                OrderNo = this.OrderNo,
                IDNo = this.IDNo,
                ItemCode = this.ItemCode,
                SpecialOrder = this.SpecialOrder,
                PaidQty = this.PaidQty,
                Qty = this.Qty,
                OriginalQty = this.OriginalQty,
                Price = this.Price,
                TaxRate = this.TaxRate,
                PrintFlag = this.PrintFlag,
                VoidFlag = this.VoidFlag,
                OrderOperator = this.OrderOperator,
                PriceSelect = this.PriceSelect,
                Condition = this.Condition,
                Seat = this.Seat,
                Discount = this.Discount,
                SentToKitchen = this.SentToKitchen,
                VoidReason = this.VoidReason,
                CheckListPrinted = this.CheckListPrinted,
                OriginalPrice = this.OriginalPrice,
                PresetDiscountCode = this.PresetDiscountCode,
                RedeemItem = this.RedeemItem,
                ManuallyEnterWeight = this.ManuallyEnterWeight
            };
            
        }

    }


}
