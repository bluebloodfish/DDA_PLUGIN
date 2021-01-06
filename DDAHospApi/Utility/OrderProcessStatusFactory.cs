using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.Utility
{


    public static class OrderProcessStatusFactory
    {
        // 1 --- 1
        //0 ---0
        //-101 --------- 2001
        //-201 ---------- 2101
        //-202 ---------- 2102
        //-203 ---------- 2103
        public static ProcessStatusInfo Success()
        {
            return new ProcessStatusInfo
            {
                Status = OrderProcessStatusEnum.Success,
                Message = "Ok",
                InnerMessage = "Ok"
            };
        }

        public static ProcessStatusInfo MergeSuccess()
        {
            return new ProcessStatusInfo
            {
                Status = OrderProcessStatusEnum.TableOrderMergeSuccess,
                Message = "OK. Table Order Merged Success",
                InnerMessage = "Ok. Table Order Merged Success"
            };
        }

        public static ProcessStatusInfo TableOccupied(string tableNo) {
            return new ProcessStatusInfo {
                Status = OrderProcessStatusEnum.TableOccupied,
                Message = $"Table {tableNo} is occupied."
            };
        }

        public static ProcessStatusInfo PosInnerError(string innerMessage, string message = "POS order is saved to POS, but printer server has no response")
        {
            return new ProcessStatusInfo
            {
                Status = OrderProcessStatusEnum.PosInnerError,
                Message = message,
                InnerMessage = innerMessage
            };
        }

        public static ProcessStatusInfo ItemCodeNotExist(string itemCode)
        {
            return new ProcessStatusInfo
            {
                Status = OrderProcessStatusEnum.ItemCodeNotExist,
                Message = $"Item_Code {itemCode} not exist"
            };
        }

        public static ProcessStatusInfo PrinterServerNoResponce()
        {
            return new ProcessStatusInfo
            {
                Status = OrderProcessStatusEnum.PrinterServerNoResponce,
                Message = "POS order is saved to POS, but printer server has no response"
            };
        }

        public static ProcessStatusInfo FailedToGetOrderNo()
        {
            return new ProcessStatusInfo
            {
                Status = OrderProcessStatusEnum.FailedToGetOrderNo,
                Message = "POS system Failed to generate new OrderNo"
            };
        }


    }

    public enum OrderProcessStatusEnum {
        TableOrderMergeSuccess = 1,
        Success = 0,
        PosInnerError = -101,
        TableOccupied = -201,
        ItemCodeNotExist = -202,
        PrinterServerNoResponce = -203,
        FailedToGetOrderNo = -204
    }

    public static class OrderProcessStatusApiReturnCode{
        public static int ApiCode(this OrderProcessStatusEnum s1)
        {
            switch (s1)
            {
                case OrderProcessStatusEnum.Success:
                    return 0;
                case OrderProcessStatusEnum.TableOrderMergeSuccess:
                    return 1;
                case OrderProcessStatusEnum.PosInnerError:
                    return 2001;
                case OrderProcessStatusEnum.TableOccupied:
                    return 2101;
                case OrderProcessStatusEnum.ItemCodeNotExist:
                    return 2102;
                case OrderProcessStatusEnum.PrinterServerNoResponce:
                    return 2103;
                case OrderProcessStatusEnum.FailedToGetOrderNo:
                    return 2104;
                default:
                    return 3000;

            }
        }
    }

    public class ProcessStatusInfo
    {
        public OrderProcessStatusEnum Status { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public string InnerMessage { get; set; }
    }

}
