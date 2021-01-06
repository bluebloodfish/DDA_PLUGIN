using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.WebApi.Model
{
    public class OnlineOrder
    {
        public string Shop_Code { get; set; }
        public int Op_Type { get; set; } //Operation Type. 1 --> Create Order, 0 --> Cancel Order
        public string Platform_Name { get; set; }
        public S_Order Order { get; set; }
        public S_Customer Customer { get; set; }
        public S_Courier Courier { get; set; }
        public List<S_Item> Items { get; set; }
        public S_Callback Callback { get; set; }
    }

    public class S_Order
    {
        public string Order_No { get; set; } //Order number from Online Ordering System [NOT NULL]
        public string Order_No_Store { get; set; }
        public long OrderDateTime { get; set; }
        public int Discount { get; set; }
        public int Delivery_Fee { get; set; } //delivery fee for order
        public int Total_Amount { get; set; } //the total amount  [NOT NULL]

        public string Table_No { get; set; } //TableOrdering

        public int Pay_Status { get; set; } //1 => PaidOnline; 2 => PaidinStore  [NOT NULL]
        public string Pay_By { get; set; } //Payment method; e.g. master/visa/eftpos ... 
        public string Order_Notes { get; set; } //Customer message

        public int Delivery_Type { get; set; } 


    }

    public class S_Customer
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

    public class S_Courier
    {
        public string Job_No { get; set; }
        public string Driver_Name { get; set; }
        public string Driver_Phone { get; set; }
    }

    public class S_Item
    {
        public string Item_Code { get; set; } //string [Not null]
        public string Item_Code_Store { get; set; }
        public string Sku { get; set; }
        public int Price_Level { get; set; }
        public int Price_Level_Store { get; set; }

        public string Item_Name1 { get; set; }
        public string Item_Name2 { get; set; }
        public bool Gst { get; set; }
        public int Qty { get; set; }
        public int Price { get; set; }
        public string Item_Notes { get; set; }
        public List<S_Instruction> Instructions { get; set; }
        //public string Sku_Store { get; set; }

    }

    public class S_Instruction
    {
        public string Item_Code { get; set; }
        public string Item_Code_Store { get; set; }
        public string Sku { get; set; }
        public int Price_Level { get; set; }
        public int Price_Level_Store { get; set; }

        public string Item_Name1 { get; set; }
        public string Item_Name2 { get; set; }
        public bool Gst { get; set; }
        public int Qty { get; set; }
        public int Price { get; set; }
    }

    public class S_Callback
    {
        public string Url { get; set; }
        public string Reference_Id { get; set; }
        public string Status { get; set; }
    }
}
