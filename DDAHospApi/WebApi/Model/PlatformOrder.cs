using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.WebApi.Model
{
    public class PlatformOrder
    {
        public string Shop_Code { get; set; }
        public string Platform_Name { get; set; } //For DDA
        public int Order_Type { get; set; } //0: Hosp Simple Order; 1: Hosp code matching order
        public P_Order Order { get; set; }
        public P_Customer Customer { get; set; }
        public P_Courier Courier { get; set; }
        public List<P_OrderItem> Items { get; set; }
        public P_Callback Callback { get; set; }
    }

    public class P_Order
    {
        public string Order_No { get; set; } //Order number from Online Ordering System [NOT NULL]
        public string Order_No_Store { get; set; } //For DDA
        public long TT_Order_Id { get; set; }
        public long Order_DateTime { get; set; }
        public long Pickup_Time { get; set; }
        public int Discount { get; set; }
        public int Tips { get; set; }
        public int Surcharge { get; set; }
        public int Delivery_Fee { get; set; } //delivery fee for order
        public int Total_Amount { get; set; } //the total amount  [NOT NULL]

        public string Table_No { get; set; } //TableOrdering

        public int Pay_Status { get; set; } //1 => PaidOnline; 2 => PaidinStore  [NOT NULL]
        public string Order_Notes { get; set; } //Customer message
        public string Delivery_Notes { get; set; } //Customer delviery message

        public int Delivery_Type { get; set; }
        public double Lng { get; set; }
        public double Lat { get; set; }

        public double GetDiscount()
        {
            return Discount / 100.0;
        }

        public double GetDeliveryFee()
        {
            return Delivery_Fee / 100.0;
        }

        public double GetTotalAmount()
        {
            return Total_Amount / 100.0;
        }

        public double GetTipsAmount()
        {
            return Tips / 100.0;
        }

        public double GetSurchargeAmount()
        {
            return Surcharge / 100.0;
        }

    }

    public class P_Customer
    {
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Telphone { get; set; }

        public string Address { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string Post_Code { get; set; }
    }

    public class P_Courier
    {
        public string Job_No { get; set; }
        public string Driver_Name { get; set; }
        public string Driver_Phone { get; set; }
    }

    public class P_OrderItem
    {
        
        public int Gst { get; set; }
        public int Qty { get; set; }
        public int Price { get; set; }
        public string Item_Name1 { get; set; }
        public string Item_Name2 { get; set; }
        public string Customer_Notes { get; set; }

        /// <summary>
        /// Without Code mapping
        /// </summary>
        public string Item_Description1 { get; set; }
        public string Item_Description2 { get; set; }

        /// <summary>
        /// with code mapping
        /// </summary>
        public string Item_Code { get; set; }
        public string Item_Code_Store { get; set; }
        public string Sku { get; set; }
        public int Price_Level { get; set; }
        public int Price_Level_Store { get; set; }
        public string OrderItem_Id { get; set; } //Hidden field.the order item id on platform system, use as order item cancellation.

        public List<P_Instruction> Instructions { get; set; }

        public int IsGst()
        {
            if (Gst > 0)
            {
                return 1;
            }
            else if (Gst == 0)
            {
                return 0;
            }
            else {
                return -1;
            }
        }
        public double GetQty()
        {
            return Qty / 100.0;
        }
        public double GetPrice()
        {
            return Price / 100.0;
        }

    }

   


    public class P_Instruction
    {
        public string Item_Code { get; set; }
        public string Item_Code_Store { get; set; }
        public string Sku { get; set; }
        public int Price_Level { get; set; }
        public int Price_Level_Store { get; set; }

        public string Item_Name1 { get; set; }
        public string Item_Name2 { get; set; }
        public int Gst { get; set; }
        public int Qty { get; set; }
        public int Price { get; set; }

        public int IsGst()
        {
            if (Gst > 0)
            {
                return 1;
            }
            else if (Gst == 0)
            {
                return 0;
            }
            else {
                return -1;
            }
        }
        public double GetQty()
        {
            return Qty / 100.0;
        }
        public double GetPrice()
        {
            return Price / 100.0;
        }
    }

    public class P_Callback
    {
        public string Url { get; set; }
        public string Reference_Id { get; set; }
        public int Status { get; set; }
    }
}
