using DDAApi.DataAccess;
using DDAApi.Security;
using DDAApi.TTOpenApi;
using DDAApi.Utility;
using DDAApi.WebApi.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DDAApi.CancelOrderQueue
{
    public class CancelOrderQueueManager : ICancelOrderQueueManager
    {
        private readonly ICancelOrderQueueProvider _provider;
        private readonly ILogger<CancelOrderQueueManager> _logger;
        private readonly IOrderHLogManager _orderHLogManager;
        private readonly DDAApiSetting _options;
        private bool _isRunning = false;
        private bool _tryStop = false;
        private Thread _thread;

        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="logger"></param>
        public CancelOrderQueueManager(ICancelOrderQueueProvider provider, ILogger<CancelOrderQueueManager> logger,
                                IOrderHLogManager orderHLogManager, IOptions<DDAApiSetting> options)
        {
            _provider = provider;
            _logger = logger;
            _orderHLogManager = orderHLogManager;
            this._options = options.Value;
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
                _logger.LogWarning("Cancel Order Processing thread is running. New thread create is abort.");
                return;
            }
            _isRunning = true;
            _thread = new Thread(StartCancelOrder)
            {
                Name = "DDACancelOrderQueue",
                IsBackground = true,
            };
            _logger.LogInformation("Cancel Order Processing thread is about to starting.");
            _thread.Start();
            _logger.LogInformation($"Cancel Order Processing thread is started，thread id is：{ _thread.ManagedThreadId}");
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

        private void StartCancelOrder()
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
                        //Tim: Get all pending orders.
                        var pendingCancelOrders = this._orderHLogManager.GetPendingOrderHLog();
                        if (pendingCancelOrders != null && pendingCancelOrders.Count() > 0)
                        {
                            foreach (var ttorderId in pendingCancelOrders)
                            {
                                _provider.Enqueue(ttorderId);
                            }
                        }
                       
                        Thread.Sleep(5000);
                        
                        continue;
                    }
                    if (_provider.TryDequeue(out string tt_OrderId))
                    {
                        _logger.LogInformation($"Start cancel order!");
                        sw.Restart();

                        var orderHLog = this._orderHLogManager.GetOrderHLog(tt_OrderId);

                        if (orderHLog != null) {
                            try
                            {
                                int retries = orderHLog.Retried + 1;

                                orderHLog.Retried = retries;

                                DDAHMACDelegatingHandler _handler = new DDAHMACDelegatingHandler(this._options.T_AppId, this._options.T_SecretKey);
                                using (HttpClient _client = HttpClientFactory.Create(_handler))
                                {

                                    HttpResponseMessage response = _client.PostAsJsonAsync($"{this._options.T_CallBackBaseUrl}cancelOrderfrompos", new { Id = tt_OrderId }).GetAwaiter().GetResult();

                                    string responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                                    ResponseFromCallback data = JsonConvert.DeserializeObject<ResponseFromCallback>(responseString);

                                    

                                    if (response.IsSuccessStatusCode)
                                    {
                                        if (data.Code == 0)
                                        {
                                            orderHLog.Message = "OK";
                                            this._logger.LogInformation($"{orderHLog.OrderNo} is canceled successfully.");
                                            orderHLog.Status = OrderHLogStatus.Success.Code;
                                            this._orderHLogManager.UpdateOrderHLog(orderHLog).GetAwaiter().GetResult();
                                        }
                                        else
                                        {
                                            var message = $"Error. CancelOrder Callback - TTOrderId: {tt_OrderId} - of is failed. Reason: {data.Message}";
                                            if (orderHLog.Retried > 10) {
                                                orderHLog.Message = message;
                                                orderHLog.Status = OrderHLogStatus.Failed.Code;
                                                this._logger.LogError(message);
                                                this._orderHLogManager.UpdateOrderHLog(orderHLog).GetAwaiter().GetResult();
                                            }
                                            else { 
                                                orderHLog.Message = message;
                                                orderHLog.Status = OrderHLogStatus.Pending.Code;
                                                this._logger.LogError(message);
                                                this._orderHLogManager.UpdateOrderHLog(orderHLog).GetAwaiter().GetResult();
                                            }
                                        }

                                    }
                                    else
                                    {
                                        var message = $"Error.  CancelOrder Callback - TTOrderId: {tt_OrderId} - failed. Reason: {response.ReasonPhrase}";

                                        if (orderHLog.Retried > 10)
                                        {
                                           
                                            orderHLog.Message = message;
                                            orderHLog.Status = OrderHLogStatus.Failed.Code;
                                            this._logger.LogError(message);
                                            this._orderHLogManager.UpdateOrderHLog(orderHLog).GetAwaiter().GetResult();
                                        }
                                        else
                                        {
                                            orderHLog.Message = message;
                                            orderHLog.Status = OrderHLogStatus.Pending.Code;
                                            this._logger.LogError(message);
                                            this._orderHLogManager.UpdateOrderHLog(orderHLog).GetAwaiter().GetResult();
                                        }
                                    }
                                }


                            }
                            catch (Exception ex)
                            {

                                var message = $"CancelOrder Callback - TTOrderId: {tt_OrderId} - failed. Reason: {ex.Message}";

                                if (orderHLog.Retried > 10)
                                {

                                    orderHLog.Message = message;
                                    orderHLog.Status = OrderHLogStatus.Failed.Code;
                                    this._logger.LogError(message);
                                    this._orderHLogManager.UpdateOrderHLog(orderHLog).GetAwaiter().GetResult();
                                }
                                else
                                {
                                    orderHLog.Message = message;
                                    orderHLog.Status = OrderHLogStatus.Pending.Code;
                                    this._logger.LogError(message);
                                    this._orderHLogManager.UpdateOrderHLog(orderHLog).GetAwaiter().GetResult();
                                }
                            }
                        }

                        sw.Stop();
                        _logger.LogInformation($"Stop Sending Order! Time elapsed: {sw.Elapsed.TotalSeconds}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "QueueManager Exception");
                }
            }
            

            _logger.LogInformation("Cancel Order processing thread is going to end without error. Exit maually!!");
            _tryStop = false;
            _isRunning = false;
        }

       



    }

   
}
