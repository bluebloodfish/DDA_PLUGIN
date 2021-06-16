using DDAApi.DataAccess;
using DDAApi.HospModel;
using DDAApi.TTOpenApi;
using DDAApi.Utility;
using DDAApi.WebApi.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DDAApi.OrderQueue
{
    public class OrderQueueManager: IOrderQueueManager
    {
        private readonly IOrderQueueProvider _provider;
        private readonly ILogger<OrderQueueManager> _logger;
        private readonly IOrderProcessor _orderProcess;
        private readonly ITT_OpenApi _tt_OpenApi;
        private readonly ITT_OrderProcess_Log_Manage _orderProcessLogManage;
        private readonly ITT_OrderNoMapping_Manage _orderNoMappingManage;
        private bool _isRunning = false;
        private bool _tryStop = false;
        private Thread _thread;

        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="logger"></param>
        public OrderQueueManager(IOrderQueueProvider provider, ILogger<OrderQueueManager> logger, 
                                IOrderProcessor orderProccessor, ITT_OpenApi tt_OpenApi,
                                ITT_OrderProcess_Log_Manage orderProcessLogManage,
                                ITT_OrderNoMapping_Manage orderNoMappingManage)
        {
            _provider = provider;
            _logger = logger;
            _orderProcess = orderProccessor;
            _tt_OpenApi = tt_OpenApi;
            _orderProcessLogManage = orderProcessLogManage;
            _orderNoMappingManage = orderNoMappingManage;
        }

        /// <summary>
        /// 正在运行
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 计数
        /// </summary>
        public int Count => _provider.Count;

        /// <summary>
        /// 启动队列
        /// </summary>
        public void Run()
        {
            if (_isRunning || (_thread != null && _thread.IsAlive))
            {
                _logger.LogWarning("Order Processing thread is running. New thread create is abort.");
                return;
            }
            _isRunning = true;
            _thread = new Thread(StartSendOrder)
            {
                Name = "DDAOrderQueue",
                IsBackground = true,
            };
            _logger.LogInformation("Order Processing thread is about to starting.");
            _thread.Start();
            _logger.LogInformation($"Order Processing thread is started，thread id is：{ _thread.ManagedThreadId}");
        }

        /// <summary>
        /// 停止队列
        /// </summary>
        public void Stop()
        {
            if (_tryStop)
            {
                return;
            }
            _tryStop = true;
        }

        private void StartSendOrder()
        {
            var sw = new Stopwatch();
            
            while (true)
            {
                try
                {
                    if (_tryStop)
                    {
                        break;
                    }

                    if (_provider.IsEmpty)
                    {
                        //_logger.LogTrace("Order queue is empty, take a sleep.");
                        Thread.Sleep(5000);
                        continue;
                    }
                    if (_provider.TryDequeue(out PlatformOrder order))
                    {
                        _logger.LogInformation($"Start Sending Order! Order from {order.Platform_Name}, order_no is {order.Order.Order_No}");
                        sw.Restart();

                        //Tim: Send Order Here
                        OrderProcessResult result = new OrderProcessResult();

                        if (order.Order_Type == 0) {
                            result = this._orderProcess.SimpleOrder(order).GetAwaiter().GetResult();
                            //_tt_OpenApi.CallbackToConfirmOrder(result.PosOrderNo, order.Order.TT_Order_Id);

                        } else if (order.Order_Type == 1 || order.Order_Type == 2) {
                            result = this._orderProcess.POSCodeOrder(order).GetAwaiter().GetResult();
                            
                        }

                        this._orderProcessLogManage.AddOrderProcessLog(new TT_OrderProcess_Log
                        {
                            DDAOrderNo = result.PosOrderNo,
                            TTOrderId = order.Order.TT_Order_Id.ToString(),
                            PlatformOrderNo = order.Order.Order_No,
                            LogDateTime = DateTime.Now,
                            Status = result.Result.Status.ApiCode(),
                            StatusNotes = result.Result.Message,
                            ErrorId = result.ErrorId,
                            JsonStr = ""
                        }).GetAwaiter().GetResult();


                        var mapping = this._orderNoMappingManage.GetMapping(result.PosOrderNo, order.Order.TT_Order_Id);
                        if (mapping == null) {
                            this._orderNoMappingManage.AddOrderNoMapping(
                                    new TT_OrderNoMapping
                                    {
                                        OrderNo = result.PosOrderNo,
                                        TTId = order.Order.TT_Order_Id,
                                        PlatName = order.Platform_Name,
                                        PlatOrderNo = order.Order.Order_No
                                    }
                           ).GetAwaiter().GetResult();
                        }
                        _tt_OpenApi.CallbackToConfirmOrder(result);

                        sw.Stop();
                        _logger.LogInformation($"Start Sending Order! Order from { order.Platform_Name}, order_no is {order.Order.Order_No}, time elapsed: {sw.Elapsed.TotalSeconds}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "QueueManager Exception");
                }
            }
            

            _logger.LogInformation("Order processing thread is going to end without error. Exit maually!!");
            _tryStop = false;
            _isRunning = false;
        }

       



    }

   
}
