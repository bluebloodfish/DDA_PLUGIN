using DDAApi.DataAccess;
using DDAApi.HospModel;
using DDAApi.Utility;
using DDAApi.WebApi.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace DDAApi.Order_Parser
{
    public class HospOrderParser: IHospOrderParser
    {
        private readonly IHospOrderManage _orderManage;
        private readonly IDDAVersionManager _versionManager;
        private readonly IConfiguration _config;
        private readonly IHospMenuManager _menuManager;
        private readonly DDAApiSetting _options;

        public HospOrderParser(IHospOrderManage orderManage,  
                                IHospMenuManager menuManager,
                                IDDAVersionManager versionManager,
                                IConfiguration config,
                                IOptions<DDAApiSetting> opitons)
        {
            this._orderManage = orderManage;
            this._versionManager = versionManager;
            this._config = config;
            this._menuManager = menuManager;
            this._options = opitons.Value;
        }


        public OrderParserResult SimpleHospOrder(PlatformOrder pOrder)
        {
            var hospOrderItems = new List<HospOrderItem>();
            var recvAcct = new HospRecvAcct();
            var orderHead = initHospOrderHead(pOrder);

            //Tim: OrderItems Parts [Starts]
            Int16 idNo = 1;
            double orderGstAmount = 0.0;

            if (pOrder.Items != null && pOrder.Items.Count > 0)
            {
                foreach (var oItem in pOrder.Items)
                {
                    var hItem = initHospOrderItem(orderHead, oItem);
                    hItem.PriceSelect = 0;
                    hItem.IDNo = idNo++;
                    hItem.ItemCode = this._options.DefaultItemCode;

                    if (this._options.PrintDescNumberForSimpleOrder == 1)
                    {
                        var itemDesc = oItem.Item_Name1;
                        if (string.IsNullOrEmpty(oItem.Item_Description1) && !string.IsNullOrEmpty(oItem.Customer_Notes))
                        {
                            itemDesc += $" [** {oItem.Customer_Notes} **]";
                        }
                        else if (!string.IsNullOrEmpty(oItem.Item_Description1) && string.IsNullOrEmpty(oItem.Customer_Notes))
                        {
                            itemDesc += $" [{oItem.Item_Description1}]";
                        }
                        else if (!string.IsNullOrEmpty(oItem.Item_Description1) && !string.IsNullOrEmpty(oItem.Customer_Notes))
                        {
                            itemDesc += $" [{oItem.Item_Description1}] [** {oItem.Customer_Notes} **]";
                        }

                        if (!string.IsNullOrEmpty(itemDesc)) {
                            hItem.SpecialOrder = FullWidthString.Get(itemDesc, false);
                        }

                        
                    }
                    else
                    {
                        var itemDesc = oItem.Item_Name2;
                        if (string.IsNullOrEmpty(oItem.Item_Description2) && !string.IsNullOrEmpty(oItem.Customer_Notes))
                        {
                            itemDesc += $" [** {oItem.Customer_Notes} **]";
                        }
                        else if (!string.IsNullOrEmpty(oItem.Item_Description2) && string.IsNullOrEmpty(oItem.Customer_Notes))
                        {
                            itemDesc += $" [{oItem.Item_Description2}]";
                        }
                        else if (!string.IsNullOrEmpty(oItem.Item_Description2) && !string.IsNullOrEmpty(oItem.Customer_Notes))
                        {
                            itemDesc += $" [{oItem.Item_Description2}] [** {oItem.Customer_Notes} **]";
                        }

                        if (!string.IsNullOrEmpty(itemDesc)) {
                            hItem.SpecialOrder = FullWidthString.Get(itemDesc, false);
                        }
                        
                    }

                    if (hItem.TaxRate > 0)
                    {
                        orderGstAmount += hItem.Qty * hItem.Price / (1 + hItem.TaxRate);
                    }

                    hospOrderItems.Add(hItem);
                }

                //orderHead.GST += orderGstAmount + pOrder.Order.GetSurchargeAmount()/11.0;
            }

            var result = convertDelivery(orderHead, pOrder);
            if (result == 1)
            {
                var deliveryItem = new HospOrderItem
                {
                    //OrderNo = orderHead.OrderNo,
                    PaidQty = 0.0,
                    OriginalQty = 0.0,
                    OriginalPrice = pOrder.Order.GetDeliveryFee(),
                    Qty = 1.0,
                    Price = pOrder.Order.GetDeliveryFee(),
                    PrintFlag = false,
                    VoidFlag = false,
                    OrderOperator = orderHead.OpName,
                    PriceSelect = 0,
                    IDNo = idNo++,
                    ItemCode = this._options.DeliveryItemCode,
                    TaxRate = 10.0,
                    SpecialOrder = "",
                    Condition = 0,
                };
                hospOrderItems.Add(deliveryItem);
                orderHead.GST = pOrder.Order.GetDeliveryFee() / 11;
            }

            if (pOrder.Order.Surcharge > 0)
            {
                var surchargeItem = new HospOrderItem
                {
                    //OrderNo = orderHead.OrderNo,
                    PaidQty = 0.0,
                    OriginalQty = 0.0,
                    OriginalPrice = pOrder.Order.GetSurchargeAmount(),
                    Qty = 1.0,
                    Price = pOrder.Order.GetSurchargeAmount(),
                    PrintFlag = false,
                    VoidFlag = false,
                    OrderOperator = orderHead.OpName,
                    PriceSelect = 0,
                    IDNo = idNo++,
                    ItemCode = this._options.SurchargeItemCode,
                    TaxRate = 10.0,
                    SpecialOrder = "",
                    Condition = 0,
                };
                hospOrderItems.Add(surchargeItem);
                orderHead.GST += pOrder.Order.GetSurchargeAmount() / 11;
            }

            if (orderHead.DollarDiscount > 0) {
                var discountGst = orderHead.DollarDiscount / 11;
                if (discountGst < orderHead.GST)
                {
                    orderHead.GST = orderHead.GST - discountGst;
                }
                else {
                    orderHead.GST = 0;
                }
            }

            var recvAcctList = new List<HospRecvAcct>();

            if (pOrder.Order.Pay_Status == 0)
            {
                orderHead.Credit = true;
                orderHead.PaidAmount = pOrder.Order.GetTotalAmount();
                //orderHead.PaidAmount = orderHead.Amount + pOrder.Order.GetTipsAmount() + pOrder.Order.GetSurchargeAmount();

                recvAcct = GetHospRecvAcct(orderHead, pOrder);
                 recvAcctList.Add(recvAcct);
            }
            else
            {
                orderHead.Credit = false;
                orderHead.PaidAmount = 0;
            }


            return new OrderParserResult
            {
                Code = 0,
                Message = "success",
                Order = new HospOrder {
                    OrderHead = orderHead,
                    OrderItems = hospOrderItems,
                    RecvAcctList = recvAcctList
                }
            };
            
        }
        


        public OrderParserResult HospOrderWPOSCode(PlatformOrder pOrder)
        {
            var hospOrderItems = new List<HospOrderItem>();
            var recvAcct = new HospRecvAcct();

            var orderHead = initHospOrderHead(pOrder);
            

            //Tim: OrderItems Parts [Starts]
            Int16 idNo = 1;
            double orderGstAmount = 0.0;

            if (pOrder.Items != null && pOrder.Items.Count > 0)
            {
                foreach (var pItem in pOrder.Items)
                {
                    if (!DoesMenuItemExist(pItem.Item_Code)) {
                        return new OrderParserResult {
                            Code = -1,
                            Message = pItem.Item_Code,
                            Order = new HospOrder()
                        };
                    }

                    var hItem = initHospOrderItem(orderHead, pItem);
                    hItem.IDNo = idNo++;
                    hItem.PriceSelect = (short)pItem.Price_Level;
                    hItem.ItemCode = pItem.Item_Code;

                    if (hItem.TaxRate > 0)
                    {
                        orderGstAmount += hItem.Qty * hItem.Price / (1 + hItem.TaxRate);
                    }

                    if (!string.IsNullOrEmpty(pItem.Customer_Notes)) {
                        hItem.SpecialOrder = FullWidthString.Get(pItem.Customer_Notes, false);
                    }
                    

                    hospOrderItems.Add(hItem);

                    if (pItem.Instructions != null && pItem.Instructions.Count > 0) {
                        foreach (var pInstruct in pItem.Instructions) {

                            if (!DoesMenuItemExist(pInstruct.Item_Code))
                            {
                                return new OrderParserResult
                                {
                                    Code = -1,
                                    Message = pInstruct.Item_Code,
                                    Order = new HospOrder()
                                };
                            }

                            var hInstruct = initHospOrderItem(orderHead, pInstruct);
                            hInstruct.IDNo = idNo++;
                            hInstruct.PriceSelect = (short)pInstruct.Price_Level_Store;
                            hInstruct.ItemCode = pInstruct.Item_Code;

                            if (hInstruct.TaxRate > 0)
                            {
                                orderGstAmount += hInstruct.Qty * hInstruct.Price / (1 + hInstruct.TaxRate);
                            }

                            hospOrderItems.Add(hInstruct);
                        }

                    }

                }

                //orderHead.GST = orderGstAmount + pOrder.Order.GetSurchargeAmount()/11.0;
            }

            var result = convertDelivery(orderHead, pOrder);
            if (result == 1) {

                var deliveryItem = new HospOrderItem
                {
                    //OrderNo = orderHead.OrderNo,
                    PaidQty = 0.0,
                    OriginalQty = 0.0,
                    OriginalPrice = pOrder.Order.GetDeliveryFee(),
                    Qty = 1.0,
                    Price = pOrder.Order.GetDeliveryFee(),
                    PrintFlag = false,
                    VoidFlag = false,
                    OrderOperator = orderHead.OpName,
                    PriceSelect = 0,
                    IDNo = idNo++,
                    ItemCode = this._options.DeliveryItemCode,
                    TaxRate = 10.0,
                    SpecialOrder = "",
                    Condition = 0,
                };
                hospOrderItems.Add(deliveryItem);
                orderHead.GST += pOrder.Order.GetDeliveryFee() / 11;
            }

            if (pOrder.Order.Surcharge > 0) {
                var surchargeItem = new HospOrderItem
                {
                    //OrderNo = orderHead.OrderNo,
                    PaidQty = 0.0,
                    OriginalQty = 0.0,
                    OriginalPrice = pOrder.Order.GetSurchargeAmount(),
                    Qty = 1.0,
                    Price = pOrder.Order.GetSurchargeAmount(),
                    PrintFlag = false,
                    VoidFlag = false,
                    OrderOperator = orderHead.OpName,
                    PriceSelect = 0,
                    IDNo = idNo++,
                    ItemCode = this._options.SurchargeItemCode,
                    TaxRate = 10.0,
                    SpecialOrder = "",
                    Condition = 0,
                };
                hospOrderItems.Add(surchargeItem);
                orderHead.GST += pOrder.Order.GetSurchargeAmount() / 11;
            }


            if (orderHead.DollarDiscount > 0)
            {
                var discountGst = orderHead.DollarDiscount / 11;
                if (discountGst < orderHead.GST)
                {
                    orderHead.GST = orderHead.GST - discountGst;
                }
                else
                {
                    orderHead.GST = 0;
                }
            }


            var recvAcctList = new List<HospRecvAcct>();
            if (pOrder.Order.Pay_Status == 0)
            {
                orderHead.Credit = true;
                //orderHead.PaidAmount = orderHead.Amount + pOrder.Order.GetTipsAmount() + pOrder.Order.GetSurchargeAmount();
                orderHead.PaidAmount = pOrder.Order.GetTotalAmount();
                recvAcct = GetHospRecvAcct(orderHead, pOrder);
                recvAcctList.Add(recvAcct);
            }
            else
            {
                orderHead.Credit = false;
                orderHead.PaidAmount = 0;
            }

            return new OrderParserResult
            {
                Code = 0,
                Message = "OK",
                Order = new HospOrder
                {
                    OrderHead = orderHead,
                    OrderItems = hospOrderItems,
                    RecvAcctList = recvAcctList
                }
            };

        }


        /// //////////////////////////////////////////////////////////////////////////////////////////////////

        private HospOrderHead initHospOrderHead(PlatformOrder pOrder) {

            int version = this._versionManager.GetDDAVersion();

            HospOrderHead orderHead;

            orderHead = new HospOrderHead
            {
                //OrderNo = this._orderManage.GetNewOrderNo(),
                OrderDate = DateTime.Now,
                OpName = pOrder.Platform_Name,
                ServicePerson = pOrder.Platform_Name,
                DollarDiscount = pOrder.Order.GetDiscount(),
                Tips = pOrder.Order.GetTipsAmount(),
                ServiceCharge = 0,
                DiscountOperator = pOrder.Platform_Name,
                BillKind = 0,
                PriceIncludesGST = true,
                CurrentGSTRate = 10.0,
                //CustomerName = getCustomerName(pOrder),
                CustomerAddress = getCustomerAddress(pOrder),
                CustomerTelephone = getCustomerPhone(pOrder),
                //Notes = getSimpleOrderNotes(pOrder),
                MachineID = pOrder.Platform_Name,
                BookingNo = "",
                VIPNo = 999999
        };

            if (version >= 8287)
            {
                orderHead.CustomerName = getCustomerName(pOrder);
                orderHead.Notes = getOrderNotes(pOrder, true);
            }
            else {
                orderHead.CustomerName = putNotesInCustomerName(pOrder);
            }

           

            if (orderHead.DollarDiscount > 0) {
                orderHead.DiscountKind = 2;
                orderHead.DiscountOperator = pOrder.Platform_Name;
            }

            if (pOrder.Order.Pickup_Time == 0)
            {
                orderHead.DueTime = null;
            }
            else
            {
                orderHead.DueTime = DateTime_Tool.GetDateTime(pOrder.Order.Pickup_Time);
            }


            if (!string.IsNullOrEmpty(pOrder.Order.Table_No))
            {
                orderHead.TableNo = pOrder.Order.Table_No;
            }
            else
            {
                orderHead.TableNo = "ONLINE";
            }

            return orderHead;
        }

        private HospOrderItem initHospOrderItem(HospOrderHead orderHead, P_OrderItem pItem) {
            var hItem = new HospOrderItem
            {
                //OrderNo = orderHead.OrderNo,
                PaidQty = 0.0,
                OriginalQty = 0.0,
                OriginalPrice = pItem.GetPrice(),
                Qty = pItem.GetQty(),
                Price = pItem.GetPrice(),
                PrintFlag = false,
                VoidFlag = false,
                OrderOperator = orderHead.OpName,
                //TaxRate = pItem.IsGst() ? 10.0 : 0.0,
                Condition = 0,
            };

            var isGst = pItem.IsGst();

            if (isGst == 1)
            {
                hItem.TaxRate = 10.0;
            }
            else if (isGst == 0)
            {
                hItem.TaxRate = 0.0;
            }
            else if (isGst == -1) {
                var item = this._menuManager.GetMenuItems()
                                .Where(x => x.ItemCode.ToUpper() == pItem.Item_Code.ToUpper())
                                .FirstOrDefault();
                hItem.TaxRate = item.TaxRate;

            }

            return hItem;
        }

        private HospOrderItem initHospOrderItem(HospOrderHead orderHead, P_Instruction pInstruct)
        {
            var hItem = new HospOrderItem
            {
                //OrderNo = orderHead.OrderNo,
                PaidQty = 0.0,
                OriginalQty = 0.0,
                OriginalPrice = pInstruct.GetPrice(),
                Qty = pInstruct.GetQty(),
                Price = pInstruct.GetPrice(),
                PrintFlag = false,
                VoidFlag = false,
                OrderOperator = orderHead.OpName,
                //TaxRate = pInstruct.IsGst() ? 10.0 : 0.0,
                Condition = 1
            };

            if (pInstruct.IsGst() == 1)
            {
                hItem.TaxRate = 10.0;
            }
            else if (pInstruct.IsGst() == 0)
            {
                hItem.TaxRate = 0.0;
            }
            else if (pInstruct.IsGst() == -1)
            {
                hItem.TaxRate = this._menuManager.GetMenuItems()
                                .Where(x => x.ItemCode.ToUpper() == pInstruct.Item_Code.ToUpper())
                                .FirstOrDefault().TaxRate;
            }

            return hItem;
        }

        private HospRecvAcct GetHospRecvAcct(HospOrderHead orderHead, PlatformOrder pOrder) {
            return new HospRecvAcct(){
                //OrderNo = orderHead.OrderNo,
                AccountDate = orderHead.OrderDate,
                PaidAmount = orderHead.PaidAmount,
                Payby = pOrder.Platform_Name.ToUpper(),
                IDNo = 1,
                OpName = orderHead.OpName,
                MachineID = pOrder.Platform_Name,
                GiftCardExpireDate = new DateTime(1900, 1, 1)

            };
           
        }

        private int convertDelivery(HospOrderHead orderHead, PlatformOrder pOrder) {
            switch (pOrder.Order.Delivery_Type)
            {
                case 1: //TIM: delivery order
                    //orderHead.Amount = pOrder.Order.GetTotalAmount() - pOrder.Order.GetTipsAmount();
                    //TIM: If DeliveryCode is not defined, delivery fee is charged by Platform.
                    //If DeliveryCode is defined, store do self-delivery, delivery fee is charged by store.
                    
                    orderHead.Delivery = true;
                    orderHead.BillKind = 2;
                    break;
                    
                case 2: //TIM: Dine-In order
                    //orderHead.Amount = pOrder.Order.GetTotalAmount() - pOrder.Order.GetTipsAmount();
                    orderHead.Delivery = false;
                    orderHead.BillKind = 0;
                    break;
                case 3: //TIM: Self-pickup order
                    //orderHead.Amount = pOrder.Order.GetTotalAmount() - pOrder.Order.GetTipsAmount();
                    orderHead.Delivery = false;
                    orderHead.BillKind = 2;
                    break;
                default:
                    //orderHead.Amount = pOrder.Order.GetTotalAmount() - pOrder.Order.GetTipsAmount();
                    break;
            }

            if (pOrder.Order.Delivery_Fee > 0)
            {
                if (string.IsNullOrEmpty(this._options.DeliveryItemCode))
                {
                    orderHead.Amount = pOrder.Order.GetTotalAmount() - pOrder.Order.GetDeliveryFee() - pOrder.Order.GetTipsAmount();
                    //orderHead.Amount = (pOrder.Order.Total_Amount - pOrder.Order.Delivery_Fee - pOrder.Order.Tips - pOrder.Order.Surcharge) / 100.0;
                    return 0;
                }
                else
                {
                    orderHead.Amount = pOrder.Order.GetTotalAmount() - pOrder.Order.GetTipsAmount();
                    return 1;

                }

            }
            else {
                orderHead.Amount = pOrder.Order.GetTotalAmount() - pOrder.Order.GetTipsAmount();
                return 0;
            }


        }

        private string putNotesInCustomerName(PlatformOrder pOrder) {
            var notes = getOrderNotes(pOrder, false);
            

            if (pOrder.Customer == null)
            {
                if (!string.IsNullOrEmpty(notes))
                {
                    return notes;
                }
                else
                {
                    return "";
                }

            }
            else {
                var customerNotes = "";
                if (!string.IsNullOrEmpty(notes) && !string.IsNullOrEmpty(pOrder.Customer.Name))
                {
                    customerNotes = $"{pOrder.Customer.Name}\n{notes}";
                }
                else if (!string.IsNullOrEmpty(notes) && string.IsNullOrEmpty(pOrder.Customer.Name))
                {
                    customerNotes = $"{notes}";
                }
                else if (string.IsNullOrEmpty(notes) && !string.IsNullOrEmpty(pOrder.Customer.Name))
                {
                    customerNotes = $"{pOrder.Customer.Name}";
                }
                return customerNotes;
            }
        }

        private string getCustomerAddress(PlatformOrder pOrder) {
            if (pOrder.Customer == null)
            {
                return "";
            }
            else {
                if (string.IsNullOrEmpty(pOrder.Customer.Address) 
                    && string.IsNullOrEmpty(pOrder.Customer.Suburb)
                    && string.IsNullOrEmpty(pOrder.Customer.State)
                    && string.IsNullOrEmpty(pOrder.Customer.Post_Code)) {
                    return "";
                }

                return $"{pOrder.Customer.Address}, {pOrder.Customer.Suburb}, {pOrder.Customer.State}, {pOrder.Customer.Post_Code}";
            }
        }

        private string getCustomerPhone(PlatformOrder pOrder)
        {
            if (pOrder.Customer == null)
            {
                return "";
            }
            else
            {
                return pOrder.Customer.Mobile;
            }
        }

        private string getCustomerName(PlatformOrder pOrder)
        {
            if (pOrder.Customer == null)
            {
                return "";
            }
            else
            {
                return pOrder.Customer.Name;
            }
        }

        private bool DoesMenuItemExist(string itemCode) {
            var item = this._menuManager.GetMenuItems()
                            .Where(x => x.ItemCode.ToUpper() == itemCode.ToUpper())
                            .FirstOrDefault();

            if (item == null)
            {
                return false;
            }
            else {
                return true;
            }
        }

        private string getOrderNotes(PlatformOrder pOrder, bool skipLinefeed)
        {
            string orderNotes = "";
            string p_notes = pOrder.Order.Order_Notes;
            string p_delivery_notes = pOrder.Order.Delivery_Notes;
            if (!string.IsNullOrEmpty(p_notes))
            {
                //orderNotes += $"[{p_notes}] \n";
                orderNotes += $"[{p_notes}]\n";
            }

            if (!string.IsNullOrEmpty(p_delivery_notes))
            {
                //orderNotes += $"[{p_notes}] \n";
                orderNotes += $"[{p_delivery_notes}]\n";
            }

            if (this._options.Print_OnlineOrderNo_inNotes == 1) {
                orderNotes += $"##{pOrder.Order.Order_No}##";
            }
            

            if (this._options.Print_OnlineOrderPayStatus_inNotes == 1 
                && pOrder.Order.Pay_Status != 0)
            {
                orderNotes += "\n[*****UNPAID*****]";
            }

            if (!string.IsNullOrEmpty(orderNotes))
            {
                return FullWidthString.Get(orderNotes, skipLinefeed);
            }
            else {
                return "";
            }
            
        }

        #region Merge Hosp Order
       
        public OrderParserResult HospMergeOrderWPOSCode(PlatformOrder pOrder, HospOrder orgHospOrder)
        {
            var additionalDDAOrderItems = new List<HospOrderItem>();

            var orderHead = updateHospOrderHead(pOrder, orgHospOrder.OrderHead);


            //Tim: OrderItems Parts [Starts]
            var orgDDAOrderItems = orgHospOrder.OrderItems;
            Int16 idNo = orgDDAOrderItems.Select(x => x.IDNo).Max(x => x);
            idNo++;
            double orderGstAmount = orderHead.GST;


            if (pOrder.Items != null && pOrder.Items.Count > 0)
            {
                foreach (var pItem in pOrder.Items)
                {
                    if (!DoesMenuItemExist(pItem.Item_Code))
                    {
                        return new OrderParserResult
                        {
                            Code = -1,
                            Message = pItem.Item_Code,
                            Order = new HospOrder()
                        };
                    }

                    var hItem = initHospOrderItem(orderHead, pItem);
                    hItem.OrderNo = orderHead.OrderNo;
                    hItem.IDNo = idNo++;
                    hItem.PriceSelect = (short)pItem.Price_Level;
                    hItem.ItemCode = pItem.Item_Code;

                    if (hItem.TaxRate > 0)
                    {
                        orderGstAmount += hItem.Qty * hItem.Price / (1 + hItem.TaxRate);
                    }

                    if (!string.IsNullOrEmpty(pItem.Customer_Notes))
                    {
                        hItem.SpecialOrder = $"Merge from {pOrder.Platform_Name} - {FullWidthString.Get(pItem.Customer_Notes, false)}";
                    }
                    else {
                        hItem.SpecialOrder = $"Merge from {pOrder.Platform_Name}";
                    }


                    additionalDDAOrderItems.Add(hItem);

                    if (pItem.Instructions != null && pItem.Instructions.Count > 0)
                    {
                        foreach (var pInstruct in pItem.Instructions)
                        {

                            if (!DoesMenuItemExist(pInstruct.Item_Code))
                            {
                                return new OrderParserResult
                                {
                                    Code = -1,
                                    Message = pInstruct.Item_Code,
                                    Order = new HospOrder()
                                };
                            }

                            var hInstruct = initHospOrderItem(orderHead, pInstruct);
                            hInstruct.OrderNo = orderHead.OrderNo;
                            hInstruct.IDNo = idNo++;
                            hInstruct.PriceSelect = (short)pInstruct.Price_Level_Store;
                            hInstruct.ItemCode = pInstruct.Item_Code;

                            if (hInstruct.TaxRate > 0)
                            {
                                orderGstAmount += hInstruct.Qty * hInstruct.Price / (1 + hInstruct.TaxRate);
                            }

                            additionalDDAOrderItems.Add(hInstruct);
                        }

                    }

                }

                orderHead.GST = orderGstAmount + pOrder.Order.GetSurchargeAmount() / 11.0;
            }

            var result = convertDelivery(orderHead, pOrder);

            double amount = 0;
            if (pOrder.Order.Delivery_Fee > 0)
            {
                if (string.IsNullOrEmpty(this._options.DeliveryItemCode))
                {
                    amount = pOrder.Order.GetTotalAmount() - pOrder.Order.GetDeliveryFee() - pOrder.Order.GetTipsAmount();
                }
                else
                {
                    amount = pOrder.Order.GetTotalAmount() - pOrder.Order.GetTipsAmount();
                    var deliveryItem = new HospOrderItem
                    {
                        OrderNo = orderHead.OrderNo,
                        PaidQty = 0.0,
                        OriginalQty = 0.0,
                        OriginalPrice = pOrder.Order.GetDeliveryFee(),
                        Qty = 1.0,
                        Price = pOrder.Order.GetDeliveryFee(),
                        PrintFlag = false,
                        VoidFlag = false,
                        OrderOperator = orderHead.OpName,
                        PriceSelect = 0,
                        IDNo = idNo++,
                        ItemCode = this._options.DeliveryItemCode,
                        TaxRate = 10.0,
                        SpecialOrder = "",
                        Condition = 0,
                    };
                    additionalDDAOrderItems.Add(deliveryItem);
                    orderHead.GST += pOrder.Order.GetDeliveryFee() / 11;
                }

            }
            else
            {
                amount = pOrder.Order.GetTotalAmount() - pOrder.Order.GetTipsAmount();
            }

            orderHead.Amount = amount + orderHead.Amount;


            if (pOrder.Order.Surcharge > 0)
            {
                var surchargeItem = new HospOrderItem
                {
                    OrderNo = orderHead.OrderNo,
                    PaidQty = 0.0,
                    OriginalQty = 0.0,
                    OriginalPrice = pOrder.Order.GetSurchargeAmount(),
                    Qty = 1.0,
                    Price = pOrder.Order.GetSurchargeAmount(),
                    PrintFlag = false,
                    VoidFlag = false,
                    OrderOperator = orderHead.OpName,
                    PriceSelect = 0,
                    IDNo = idNo++,
                    ItemCode = this._options.SurchargeItemCode,
                    TaxRate = 10.0,
                    SpecialOrder = "",
                    Condition = 0,
                };
                additionalDDAOrderItems.Add(surchargeItem);
                orderHead.GST += pOrder.Order.GetSurchargeAmount() / 11;
            }

            if (orderHead.DollarDiscount > 0)
            {
                var discountGst = orderHead.DollarDiscount / 11;
                if (discountGst < orderHead.GST)
                {
                    orderHead.GST = orderHead.GST - discountGst;
                }
                else
                {
                    orderHead.GST = 0;
                }
            }

            return new OrderParserResult
            {
                Code = 0,
                Message = "OK",
                Order = new HospOrder
                {
                    OrderHead = orderHead,
                    OrderItems = additionalDDAOrderItems,
                    RecvAcctList = null
                }
            };

        }

        private HospOrderHead updateHospOrderHead(PlatformOrder pOrder, HospOrderHead orderHead)
        {

            int version = this._versionManager.GetDDAVersion();



            orderHead.OpName = pOrder.Platform_Name;
            //orderHead.ServicePerson = pOrder.Platform_Name;

            var discount = pOrder.Order.GetDiscount();
            if (discount > 0) {
                orderHead.DollarDiscount = orderHead.DollarDiscount + discount;
            }
            orderHead.Tips = orderHead.Tips + pOrder.Order.GetTipsAmount();
            orderHead.CustomerAddress = getCustomerAddress(pOrder);
            orderHead.CustomerTelephone = getCustomerPhone(pOrder);
            orderHead.BookingNo = "";
            orderHead.VIPNo = 999999;


            if (version >= 8287)
            {
                orderHead.CustomerName = getCustomerName(pOrder);
                orderHead.Notes = getOrderNotes(pOrder, true);
            }
            else
            {
                orderHead.CustomerName = putNotesInCustomerName(pOrder);
            }



            if (orderHead.DollarDiscount > 0)
            {
                orderHead.DiscountKind = 2;
                orderHead.DiscountOperator = pOrder.Platform_Name;
            }

            if (pOrder.Order.Pickup_Time == 0)
            {
                orderHead.DueTime = null;
            }
            else
            {
                orderHead.DueTime = DateTime_Tool.GetDateTime(pOrder.Order.Pickup_Time);
            }


            if (!string.IsNullOrEmpty(pOrder.Order.Table_No))
            {
                orderHead.TableNo = pOrder.Order.Table_No;
            }
            else
            {
                orderHead.TableNo = pOrder.Platform_Name;
            }

            return orderHead;
        }

        #endregion

    }

    public class OrderParserResult
    {
        //code = 0 --> success
        //code = -1 --> item code not found
        public int Code { get; set; }
        public string Message { get; set; }
        public HospOrder Order { get; set; }
    }

}
