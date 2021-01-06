using DDAApi.DataAccess;
using DDAApi.HospModel;
using DDAApi.Order_Parser;
using DDAApi.OrderNoQueue;
using DDAApi.Utility;
using DDAApi.WebApi.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.OrderQueue
{
    

    public class OrderProccessor: IOrderProcessor
    {
        private readonly ILogger<OrderProccessor> _logger;
        private readonly DDAApiSetting _options;
        private readonly IHospOrderParser _hospOrderParser;

        private readonly IHospOrderManage _orderManage;
        private readonly IHospTableManage _tableManage;
        private readonly IDDAVersionManager _versionManager;
        private readonly IOrderNoQueueProvider _orderNoQueueProvider;

        public OrderProccessor(ILogger<OrderProccessor> logger,
                                IHospOrderManage orderManage,
                                IOptions<DDAApiSetting> options, 
                                IHospOrderParser hospOrderParser,
                                IHospTableManage tableManage,
                                IDDAVersionManager versionManager, 
                                IOrderNoQueueProvider orderNoQueueProvider)
        {
            this._logger = logger;
            this._options = options.Value;
            this._hospOrderParser = hospOrderParser;
            this._orderManage = orderManage;
            this._tableManage = tableManage;
            this._versionManager = versionManager;
            this._orderNoQueueProvider = orderNoQueueProvider;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pOrder"></param>
        /// OrderProcessResult
        /// Code = -101, failed to convert data
        /// 
        /// Code = -201，Table is not avalaible
        /// Code = -202, Item Code not found in database;
        /// <returns></returns>


        public async Task<OrderProcessResult> SimpleOrder(PlatformOrder pOrder)
        {
            if (!isTableAvaliableForOrder(pOrder))
            {
                var errorId = TokenFactory.GenerateErrorId();
                this._logger.LogError($"{errorId} - Order Error: PlatOrderNo: {pOrder.Order.Order_No} from {pOrder.Platform_Name}, Table: {pOrder.Order.Table_No} is occupied!!");
                var result = OrderProcessStatusFactory.TableOccupied(pOrder.Order.Table_No);
                return new OrderProcessResult { Result = result,
                                                PosOrderNo = "",
                                                TTOrderId = pOrder.Order.TT_Order_Id };
            }


            var parserResult = this._hospOrderParser.SimpleHospOrder(pOrder);
            HospOrder hospOrder = new HospOrder();
            if (parserResult.Code == 0)
            {
                hospOrder = parserResult.Order;
            }
            else {
                var result = OrderProcessStatusFactory.PosInnerError(innerMessage: "Failed to convert order data.");
                return new OrderProcessResult { Result = result,                                              
                                                PosOrderNo = "",
                                                TTOrderId = pOrder.Order.TT_Order_Id };
            }

            try
            {
                var orderNo = this._orderNoQueueProvider.GetNewOrderNo();

                if (string.IsNullOrEmpty(orderNo))
                {
                    var errorId = TokenFactory.GenerateErrorId();
                    this._logger.LogError($"{errorId} - Order Error: PlatOrderNo: {pOrder.Order.Order_No} from {pOrder.Platform_Name}, faild to save order due to get New Order Number failed.");
                    return new OrderProcessResult
                    {
                        Result = OrderProcessStatusFactory.FailedToGetOrderNo(),
                        PosOrderNo = "",
                        TTOrderId = pOrder.Order.TT_Order_Id,
                        ErrorId = errorId
                    };
                }
                else
                {
                    assignOrderNo(hospOrder, orderNo);
                }

                int r = await _orderManage.SaveOrder(hospOrder);

                int version = this._versionManager.GetDDAVersion();
                if (version >= 8287) {
                   await _orderManage.UpdateNotesfor8287(hospOrder.OrderHead.Notes, hospOrder.OrderHead.OrderNo);
                }

                if (r > 0)
                {
                    try {
                        if (!string.IsNullOrEmpty(this._options.PrinterServer))
                        {
                            await Print_Util.PrintDocket(this._options.PrinterServer, this._options.PrinterServerPort, orderNo, 0);
                        }

                        if (this._options.AutoPrintBill == 1) {
                            await Print_Util.PrintDocket(this._options.PrinterServer, this._options.PrinterServerPort, orderNo, 1);
                        }

                        return new OrderProcessResult { Result = OrderProcessStatusFactory.Success(),  PosOrderNo = orderNo, TTOrderId = pOrder.Order.TT_Order_Id };
                    }
                    catch (Exception e) {

                        var errorId = TokenFactory.GenerateErrorId();
                        this._logger.LogError($"{errorId} - {e.Message}");
                        var result = OrderProcessStatusFactory.PrinterServerNoResponce();
                        return new OrderProcessResult { Result = result,
                                                        PosOrderNo = orderNo,
                                                        TTOrderId = pOrder.Order.TT_Order_Id,
                                                        ErrorId = errorId };
                    }
                }
                else
                {
                    var errorId = TokenFactory.GenerateErrorId();
                    this._logger.LogError($"{errorId} - Failed to save order info to database.");
                    var result = OrderProcessStatusFactory.PosInnerError(innerMessage: "Failed to save order info to database.");
                    return new OrderProcessResult { Result = result,
                                                    PosOrderNo = "",
                                                    TTOrderId = pOrder.Order.TT_Order_Id,
                                                    ErrorId = errorId };
                }
                
                   

            }
            catch (Exception e)
            {
                var errorId = TokenFactory.GenerateErrorId();
                this._logger.LogError($"{errorId} - {e.Message}");
                var result = OrderProcessStatusFactory.PosInnerError(innerMessage: "");
                return new OrderProcessResult { Result = result,
                                                PosOrderNo = "",
                                                TTOrderId = pOrder.Order.TT_Order_Id,
                                                ErrorId = errorId }; ;

            }

        }

        public async Task<OrderProcessResult> POSCodeOrder(PlatformOrder pOrder)
        {
            OrderParserResult parserResult = null;

            bool isMergeOrder = false;

            if (!isTableAvaliableForOrder(pOrder))
            {
                //Table is occupied.

                //Auto reject for order if table is occupied.
                if (this._options.OrderForOccupiedTable == 0)
                {
                    var errorId = TokenFactory.GenerateErrorId();
                    var result = OrderProcessStatusFactory.TableOccupied(pOrder.Order.Table_No);
                    this._logger.LogError($"{errorId} - Order Error: PlatOrderNo: {pOrder.Order.Order_No} from {pOrder.Platform_Name}, Table: {pOrder.Order.Table_No} is occupied");

                    return new OrderProcessResult
                    {
                        Result = result,
                        PosOrderNo = "",
                        TTOrderId = pOrder.Order.TT_Order_Id,
                        ErrorId = errorId
                    };

                }
                //Merge additional order for the occupied table
                else if (this._options.OrderForOccupiedTable > 0)
                {
                    var orgHospOrder = this._orderManage.GetOccupiedTalbeOrder(pOrder.Order.Table_No);
                    parserResult = this._hospOrderParser.HospMergeOrderWPOSCode(pOrder, orgHospOrder);
                    isMergeOrder = true;
                }
            }
            else {

                parserResult = this._hospOrderParser.HospOrderWPOSCode(pOrder);
                isMergeOrder = false;
            }

            
            HospOrder hospOrder = new HospOrder();
            if (parserResult.Code == 0)
            {
                hospOrder = parserResult.Order;
            }
            else if (parserResult.Code == -1) {
                var errorId = TokenFactory.GenerateErrorId();
                var result = OrderProcessStatusFactory.ItemCodeNotExist(parserResult.Message);
                this._logger.LogError($"{errorId} - Order Error: PlatOrderNo: {pOrder.Order.Order_No} from {pOrder.Platform_Name}, {result.Message}");
                return new OrderProcessResult { Result = result,
                                                PosOrderNo = "",
                                                TTOrderId = pOrder.Order.TT_Order_Id,
                                                ErrorId = errorId
                                             };

            }
            else
            {
                var errorId = TokenFactory.GenerateErrorId();
                this._logger.LogError($"{errorId} - Order Error: PlatOrderNo: {pOrder.Order.Order_No} from {pOrder.Platform_Name}, {parserResult.Message}");
                var result = OrderProcessStatusFactory.PosInnerError(innerMessage: "");
                return new OrderProcessResult { Result = result,
                                                PosOrderNo = "",
                                                TTOrderId = pOrder.Order.TT_Order_Id,
                                                ErrorId = errorId};
            }


            string orderNo = "";
            int r = 0;
            try
            {
                if (!isMergeOrder)
                {
                    orderNo = this._orderNoQueueProvider.GetNewOrderNo();

                    if (string.IsNullOrEmpty(orderNo))
                    {
                        var errorId = TokenFactory.GenerateErrorId();
                        this._logger.LogError($"{errorId} - Order Error: PlatOrderNo: {pOrder.Order.Order_No} from {pOrder.Platform_Name}, faild to save order due to get New Order Number failed.");
                        return new OrderProcessResult
                        {
                            Result = OrderProcessStatusFactory.FailedToGetOrderNo(),
                            PosOrderNo = "",
                            TTOrderId = pOrder.Order.TT_Order_Id,
                            ErrorId = errorId
                        };
                    }
                    else
                    {
                        assignOrderNo(hospOrder, orderNo);
                    }

                    r = await _orderManage.SaveOrder(hospOrder);
                    //if (r > 0)
                    //{
                    //    try
                    //    {
                    //        if (!string.IsNullOrEmpty(this._options.PrinterServer))
                    //        {
                    //            await Print_Util.PrintDocket(this._options.PrinterServer, this._options.PrinterServerPort, orderNo, 0);
                    //        }

                    //        if (this._options.AutoPrintBill == 1)
                    //        {
                    //            await Print_Util.PrintDocket(this._options.PrinterServer, this._options.PrinterServerPort, orderNo, 1);
                    //        }

                    //        return new OrderProcessResult
                    //        {
                    //            Result = OrderProcessStatusFactory.Success(),
                    //            PosOrderNo = orderNo,
                    //            TTOrderId = pOrder.Order.TT_Order_Id
                    //        };
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        var errorId = TokenFactory.GenerateErrorId();
                    //        this._logger.LogError($"{errorId} - {e.Message}");
                    //        var result = OrderProcessStatusFactory.PrinterServerNoResponce();
                    //        return new OrderProcessResult
                    //        {
                    //            Result = result,
                    //            PosOrderNo = orderNo,
                    //            TTOrderId = pOrder.Order.TT_Order_Id,
                    //            ErrorId = errorId,
                    //        };

                    //    }
                    //}
                    //else
                    //{
                    //    var errorId = TokenFactory.GenerateErrorId();
                    //    var result = OrderProcessStatusFactory.PosInnerError(innerMessage: "");
                    //    this._logger.LogError($"{errorId} - Failed to save order info to database.");
                    //    return new OrderProcessResult
                    //    {
                    //        Result = result,
                    //        PosOrderNo = "",
                    //        TTOrderId = pOrder.Order.TT_Order_Id,
                    //        ErrorId = errorId
                    //    };
                    //}
                }
                else {
                    orderNo = hospOrder.OrderHead.OrderNo;
                    r = await _orderManage.MergeOrder(hospOrder);
                }

                if (r > 0)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(this._options.PrinterServer))
                        {
                            await Print_Util.PrintDocket(this._options.PrinterServer, this._options.PrinterServerPort, orderNo, 0);
                        }

                        if (this._options.AutoPrintBill == 1)
                        {
                            await Print_Util.PrintDocket(this._options.PrinterServer, this._options.PrinterServerPort, orderNo, 1);
                        }

                        if (!isMergeOrder)
                        {
                            return new OrderProcessResult
                            {
                                Result = OrderProcessStatusFactory.Success(),
                                PosOrderNo = orderNo,
                                TTOrderId = pOrder.Order.TT_Order_Id
                            };
                        }
                        else {
                            return new OrderProcessResult
                            {
                                Result = OrderProcessStatusFactory.MergeSuccess(),
                                PosOrderNo = orderNo,
                                TTOrderId = pOrder.Order.TT_Order_Id
                            };
                        }
                        
                    }
                    catch (Exception e)
                    {
                        var errorId = TokenFactory.GenerateErrorId();
                        this._logger.LogError($"{errorId} - {e.Message}");
                        var result = OrderProcessStatusFactory.PrinterServerNoResponce();
                        return new OrderProcessResult
                        {
                            Result = result,
                            PosOrderNo = orderNo,
                            TTOrderId = pOrder.Order.TT_Order_Id,
                            ErrorId = errorId,
                        };

                    }
                }
                else
                {
                    var errorId = TokenFactory.GenerateErrorId();
                    var result = OrderProcessStatusFactory.PosInnerError(innerMessage: "");
                    this._logger.LogError($"{errorId} - Failed to save order info to database.");
                    return new OrderProcessResult
                    {
                        Result = result,
                        PosOrderNo = "",
                        TTOrderId = pOrder.Order.TT_Order_Id,
                        ErrorId = errorId
                    };
                }



            }
            catch (Exception e)
            {
                var errorId = TokenFactory.GenerateErrorId();
                this._logger.LogError($"{errorId} - {e.Message}");
                var result = OrderProcessStatusFactory.PosInnerError(innerMessage: "");
                return new OrderProcessResult { Result = result,
                                                PosOrderNo = "",
                                                TTOrderId = pOrder.Order.TT_Order_Id,
                                                ErrorId = errorId
                                                }; 

            }

        }

        private void assignOrderNo(HospOrder order, string orderNo) {
            if (order.OrderHead != null) {
                order.OrderHead.OrderNo = orderNo;
            }

            if (order.OrderItems != null && order.OrderItems.Count > 0) {
                foreach (var item in order.OrderItems) {
                    item.OrderNo = orderNo;
                }
            }

            if (order.RecvAcctList != null && order.RecvAcctList.Count > 0) {
                foreach (var item in order.RecvAcctList) {
                    item.OrderNo = orderNo;
                }
            }

        }

        private bool isTableAvaliableForOrder(PlatformOrder pOrder)
        {
            if (pOrder.Order.Delivery_Type == 2
                    && pOrder.Order.Pay_Status == 1
                    && !string.IsNullOrEmpty(pOrder.Order.Table_No))
            {
                if (!this._tableManage.IsTableAvaliable(pOrder.Order.Table_No))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
    }

}
