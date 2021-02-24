using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DDAApi.HospModel;
using DDAApi.Security;
using DDAApi.DataAccess;
using DDAApi.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using DDAApi.WebApi.Model;
using DDAApi.OrderQueue;
using Microsoft.Extensions.Options;
using DDAApi.Order_Parser;
using Microsoft.Extensions.DependencyInjection;
using DDAApi.TTOpenApi;
using Microsoft.EntityFrameworkCore;
using DDAApi.CancelOrderQueue;
using DDAApi.OrderNoQueue;

namespace DDAApi.WebApi
{
    [Route("api_v1/[controller]")]
    [ApiController]
    public class HospOrderController : ControllerBase
    {
        public ILogger<HospOrderController> _logger { get; }
        private readonly IHospTableManage _tableManage;
        private readonly IConfiguration _config;
        private readonly IHospOrderManage _orderManage;
        private readonly IOrderQueueService _queueService;
        private readonly DDAApiSetting _options;
        private readonly IHospOrderParser _hospOrderParse;
        private readonly IDDAVersionManager _versionManager;
        private readonly ICancelOrderQueueService _cancelOrderQueueService;
        private readonly IOrderProcessor _orderProcessor;
        private readonly IOrderNoQueueProvider _orderNoQueueProvider;
        private readonly ITT_OrderProcess_Log_Manage _orderProcessLogManage;
        private readonly ITT_OrderNoMapping_Manage _orderNoMappingManage;
        private readonly IOrderHLogManager _orderHLog;
        private readonly AppDbContext _ctx;
        private readonly IServiceCollection _serices;
        

        public HospOrderController(ILogger<HospOrderController> logger,
                                   IDDAVersionManager versionManager,
                                   IConfiguration config,
                                   IHospOrderManage orderManage,
                                   IHospTableManage tableManage,
                                   IOrderQueueService queueService,
                                   IOptions<DDAApiSetting> options,
                                   IHospOrderParser hospOrderParser,
                                   ICancelOrderQueueService cancelOrderQueueService,
                                   IOrderProcessor orderProcessor, 
                                   IOrderNoQueueProvider orderNoQueueProvider,
                                   ITT_OrderProcess_Log_Manage orderProcessLogManage,
                                   ITT_OrderNoMapping_Manage orderNoMapping_Manage,
                                   IOrderHLogManager orderHLog)
        {
            this._logger = logger;
            this._config = config;
            this._orderManage = orderManage;
            this._tableManage = tableManage;
            this._queueService = queueService;
            this._options = options.Value;
            this._hospOrderParse = hospOrderParser;
            this._versionManager = versionManager;
            this._cancelOrderQueueService = cancelOrderQueueService;
            this._orderProcessor = orderProcessor;
            this._orderNoQueueProvider = orderNoQueueProvider;
            this._orderProcessLogManage = orderProcessLogManage;
            this._orderNoMappingManage = orderNoMapping_Manage;
            this._orderHLog = orderHLog;
        }

        [HttpGet("GetVersion")]
        public IActionResult GetVersion()
        {
            return Ok(new { code = 0, data = new { Version = "3.3.6" } });
        }


        [ServiceFilter(typeof(AuthenFilter))]
        [HttpPost("OrderWithPosCode")]
        public async Task<IActionResult> OrderWithPosCode([FromBody] PlatformOrder pOrder)
        {
            var json = JsonConvert.SerializeObject(pOrder);
            this._logger.LogInformation($"*********OrderWithPosCode******** \n {json} \n***************************");
            try
            {
                await this._orderProcessLogManage.AddOrderProcessLog(new TT_OrderProcess_Log
                {
                    DDAOrderNo = "",
                    PlatformOrderNo = pOrder.Order.Order_No,
                    TTOrderId = pOrder.Order.TT_Order_Id.ToString(),
                    LogDateTime = DateTime.Now,
                    Status = 90002,
                    StatusNotes = "Received-CodeMapping",
                    ErrorId = ""
                });

                OrderProcessResult result = new OrderProcessResult();
                result = await this._orderProcessor.POSCodeOrder(pOrder);

               
                await this._orderProcessLogManage.AddOrderProcessLog(new TT_OrderProcess_Log {
                    DDAOrderNo = result.PosOrderNo,
                    PlatformOrderNo = pOrder.Order.Order_No,
                    TTOrderId = pOrder.Order.TT_Order_Id.ToString(),
                    LogDateTime = DateTime.Now,
                    Status = result.Result.Status.ApiCode(),
                    StatusNotes = result.Result.Message,
                    ErrorId = result.ErrorId                   
                });

                var mapping = this._orderNoMappingManage.GetMapping(result.PosOrderNo, pOrder.Order.TT_Order_Id);
                if (mapping == null)
                {
                    await this._orderNoMappingManage.AddOrderNoMapping(
                             new TT_OrderNoMapping
                             {
                                 OrderNo = result.PosOrderNo,
                                 TTId = pOrder.Order.TT_Order_Id,
                                 PlatOrderNo = pOrder.Order.Order_No,
                                 PlatName = pOrder.Platform_Name
                             }
                    );
                 }

                switch (result.Result.Status)
                {
                    case OrderProcessStatusEnum.Success:
                        return Ok(new { code = result.Result.Status.ApiCode(), message = result.Result.Message, data = new { OrderNo = result.PosOrderNo } });
                    case OrderProcessStatusEnum.TableOrderMergeSuccess:
                        return Ok(new { code = result.Result.Status.ApiCode(), message = result.Result.Message, data = new { OrderNo = result.PosOrderNo } });
                    case OrderProcessStatusEnum.PosInnerError:
                        return Ok(new { code = result.Result.Status.ApiCode(), message = $"{result.Result.Message} - {result.ErrorId}", data = new { OrderNo = "" } });
                    case OrderProcessStatusEnum.TableOccupied: //Table not avaliable
                        return Ok(new { code = result.Result.Status.ApiCode(), message = $"{result.Result.Message} - {result.ErrorId}", data = new { OrderNo = "" } });
                    case OrderProcessStatusEnum.ItemCodeNotExist: //ItemCode not found
                        return Ok(new { code = result.Result.Status.ApiCode(), message = $"{result.Result.Message} - {result.ErrorId}", data = new { OrderNo = "" } });
                    case OrderProcessStatusEnum.PrinterServerNoResponce:
                        return Ok(new { code = result.Result.Status.ApiCode(), message = $"{result.Result.Message} - {result.ErrorId}", data = new { OrderNo = result.PosOrderNo } });
                    case OrderProcessStatusEnum.FailedToGetOrderNo:
                        return Ok(new { code = result.Result.Status.ApiCode(), message = $"{result.Result.Message} - {result.ErrorId}", data = new { OrderNo = ""} });
                    default:
                        return Ok(new { code = result.Result.Status.ApiCode(), message = $"Unknow Error. -  {result.ErrorId}", data = new { OrderNo = "" } });

                }
            }
            catch (Exception e) {
                this._logger.LogError(e.Message);
                return Ok(new { code = 2001, message = "Server Error", data = new { OrderNo = "" } });
            }
        }


        [ServiceFilter(typeof(AuthenFilter))]
        [HttpPost("SimpleOrder")]
        public async Task<IActionResult> SimpleOrder([FromBody] PlatformOrder pOrder)
        {
            try {

                await this._orderProcessLogManage.AddOrderProcessLog(new TT_OrderProcess_Log
                {
                    DDAOrderNo = "",
                    PlatformOrderNo = pOrder.Order.Order_No,
                    TTOrderId = pOrder.Order.TT_Order_Id.ToString(),
                    LogDateTime = DateTime.Now,
                    Status = 90001,
                    StatusNotes = "Received-SimpleOrder",
                    ErrorId = ""
                });

                OrderProcessResult result = new OrderProcessResult();
                result = await this._orderProcessor.SimpleOrder(pOrder);

                await this._orderProcessLogManage.AddOrderProcessLog(new TT_OrderProcess_Log
                {
                    DDAOrderNo = result.PosOrderNo,
                    PlatformOrderNo = pOrder.Order.Order_No,
                    TTOrderId = pOrder.Order.TT_Order_Id.ToString(),
                    LogDateTime = DateTime.Now,
                    Status = result.Result.Status.ApiCode(),
                    StatusNotes = result.Result.Message,
                    ErrorId = result.ErrorId
                });


                var mapping = this._orderNoMappingManage.GetMapping(result.PosOrderNo, pOrder.Order.TT_Order_Id);
                if (mapping == null)
                {
                    await this._orderNoMappingManage.AddOrderNoMapping(
                             new TT_OrderNoMapping
                             {
                                 OrderNo = result.PosOrderNo,
                                 TTId = pOrder.Order.TT_Order_Id,
                                 PlatName = pOrder.Platform_Name,
                                 PlatOrderNo = pOrder.Order.Order_No
                             }
                    );
                }

                switch (result.Result.Status)
                {
                    case OrderProcessStatusEnum.Success:
                        return Ok(new { code = result.Result.Status.ApiCode(), message = result.Result.Message, data = new { OrderNo = result.PosOrderNo } });
                    case OrderProcessStatusEnum.TableOrderMergeSuccess:
                        return Ok(new { code = result.Result.Status.ApiCode(), message = result.Result.Message, data = new { OrderNo = result.PosOrderNo } });
                    case OrderProcessStatusEnum.PosInnerError:
                        return Ok(new { code = result.Result.Status.ApiCode(), message = $"{result.Result.Message} - {result.ErrorId}", data = new { OrderNo = "" } });
                    case OrderProcessStatusEnum.TableOccupied: //Table not avaliable
                        return Ok(new { code = result.Result.Status.ApiCode(), message = $"{result.Result.Message} - {result.ErrorId}", data = new { OrderNo = "" } });
                    case OrderProcessStatusEnum.ItemCodeNotExist: //ItemCode not found
                        return Ok(new { code = result.Result.Status.ApiCode(), message = $"{result.Result.Message} - {result.ErrorId}", data = new { OrderNo = "" } });
                    case OrderProcessStatusEnum.PrinterServerNoResponce:
                        return Ok(new { code = result.Result.Status.ApiCode(), message = $"{result.Result.Message} - {result.ErrorId}", data = new { OrderNo = result.PosOrderNo } });
                    case OrderProcessStatusEnum.FailedToGetOrderNo:
                        return Ok(new { code = result.Result.Status.ApiCode(), message = $"{result.Result.Message} - {result.ErrorId}", data = new { OrderNo = "" } });
                    default:
                        return Ok(new { code = result.Result.Status.ApiCode(), message = $"Unknow Error. -  {result.ErrorId}", data = new { OrderNo = "" } });
                    
                }
            }
            catch (Exception e)
            {
                var errId = TokenFactory.GenerateErrorId();
                this._logger.LogError($"Errid-{errId} : {e.Message}");
                return Ok(new { code = 2001, message = "Server Error", data = new { OrderNo = "" } });
            }
            
        }


        [ServiceFilter(typeof(AuthenFilter))]
        [HttpPost("AddOrder")]
        public async Task<IActionResult> AddOrder([FromBody] PlatformOrder pOrder)
        {
            var json = JsonConvert.SerializeObject(pOrder);
            this._logger.LogInformation($"*********Add Order******** \n {json} \n***************************");

            try
            {
                await this._orderProcessLogManage.AddOrderProcessLog(new TT_OrderProcess_Log
                {
                    DDAOrderNo = "",
                    PlatformOrderNo = pOrder.Order.Order_No,
                    TTOrderId = pOrder.Order.TT_Order_Id.ToString(),
                    LogDateTime = DateTime.Now,
                    Status = 90003,
                    StatusNotes = "Received-OrderQueue",
                    ErrorId = ""
                });

                this._queueService.Enqueue(pOrder);
                return Ok(new { code = 0, message = "OK" });
            }
            catch (Exception ex)
            {
                var errId = TokenFactory.GenerateErrorId();
                this._logger.LogError($"Errid-{errId} : {ex.Message}");

                return Ok(new { code = 2001, message = ex.Message });
            }

        }


        #region MISC Apis
        [HttpGet("GetOrderNo/{id}")]
        public IActionResult GetOrderNo(int id)
        {
            var orderno = this._orderNoQueueProvider.GetNewOrderNo();
            if (!string.IsNullOrEmpty(orderno))
            {

                this._logger.LogInformation($"Manully - {id} - {orderno}");
            }
            else
            {
                this._logger.LogError($"Manully - {id} - empty");
            }

            return Ok(orderno);
        }

        [HttpGet("AutoGetOrderNo/{id}")]
        public IActionResult AutoGetOrderNo(int id)
        {
            var orderno = this._orderNoQueueProvider.GetNewOrderNo();
            if (!string.IsNullOrEmpty(orderno))
            {

                this._logger.LogInformation($"Auto - {id} - {orderno}");
            }
            else
            {
                this._logger.LogError($"Auto - {id} - empty");
            }

            return Ok(orderno);
        }

        [ServiceFilter(typeof(LocalRequestOnlyFilter))]
        [HttpPost("CancelOrder")]
        public IActionResult CancelOrder([FromBody] CancelOrderRequest cancelOrderReqeust)
        {
            var orderNo = cancelOrderReqeust.OrderNo;

            try
            {
                var ttIdList = this._orderHLog.GetOrderHLogs(orderNo);
                this._logger.LogInformation("CancelOrder API running");
                if (ttIdList != null && ttIdList.Count() > 0)
                {
                    foreach (var orderId in ttIdList)
                    {
                        this._cancelOrderQueueService.Enqueue(orderId);
                    }
                }
                this._logger.LogInformation("CancelOrder API before return");
                return Ok(new { code = 0, message = "OK" });
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message);
                return Ok(new { code = 2001, message = ex.Message });
            }

        }

        public class CancelOrderRequest
        {
            public string OrderNo { get; set; }

        }


        [HttpGet("IsTableAvaliable/{tableNo}")]
        public IActionResult IsTableAvaliable(string tableNo)
        {
            if (string.IsNullOrEmpty(tableNo))
            {
                return Ok(new { code = 2001, message = "table number cannot be empty!" });
            }

            if (this._tableManage.IsTableAvaliable(tableNo))
            {
                return Ok(new { code = 0, data = new { Avaliable = 1 }, message = $"Table {tableNo} is currently available!!" });
            }
            else
            {
                return Ok(new { code = 0, data = new { Avaliable = 0 }, message = $"Table {tableNo} is currently not available!!" });

            }
        }

        private bool IsTableAvaliableForOrder(PlatformOrder pOrder)
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
        #endregion





    }
}