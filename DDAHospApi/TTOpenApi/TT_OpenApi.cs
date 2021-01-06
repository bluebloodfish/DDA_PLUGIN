using DDAApi.OrderQueue;
using DDAApi.Security;
using DDAApi.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.TTOpenApi
{

    public class TT_OpenApi: ITT_OpenApi
    {
        private readonly DDAApiSetting _options;
        private readonly ILogger<TT_OpenApi> _logger;

        public TT_OpenApi(IOptions<DDAApiSetting> options, ILogger<TT_OpenApi> logger)
        {
            this._options = options.Value;
            this._logger = logger;
        }

        public async Task CallbackToConfirmOrder(OrderProcessResult orderResult)//)string ddaOrderNo, int ttOrderNo)
        {
            try
            {
                DDAHMACDelegatingHandler _handler = new DDAHMACDelegatingHandler(this._options.T_AppId, this._options.T_SecretKey);
                using (HttpClient _client = HttpClientFactory.Create(_handler))
                {
                    var request = new RequestToCallback();
                    switch (orderResult.Result.Status) {
                        case OrderProcessStatusEnum.Success:
                            request = new RequestToCallback
                            {
                                Code = orderResult.Result.Status.ApiCode(),
                                Message = "OK",
                                POSOrderNo = orderResult.PosOrderNo,
                                OrderId = orderResult.TTOrderId
                            };
                            break;
                        case OrderProcessStatusEnum.TableOrderMergeSuccess:
                            request = new RequestToCallback
                            {
                                Code = orderResult.Result.Status.ApiCode(),
                                Message = orderResult.Result.Message,
                                POSOrderNo = orderResult.PosOrderNo,
                                OrderId = orderResult.TTOrderId
                            };
                            break;
                        case OrderProcessStatusEnum.PosInnerError:
                            request = new RequestToCallback
                            {
                                Code = orderResult.Result.Status.ApiCode(),
                                Message = $"{orderResult.Result.Message} - {orderResult.ErrorId}",
                                POSOrderNo = "",
                                OrderId = orderResult.TTOrderId
                            };
                            break;
                        case OrderProcessStatusEnum.TableOccupied: //Table not avaliable
                            request = new RequestToCallback
                            {
                                Code = orderResult.Result.Status.ApiCode(),
                                Message = $"{orderResult.Result.Message} - {orderResult.ErrorId}",
                                POSOrderNo = "",
                                OrderId = orderResult.TTOrderId
                            };
                            break;
                        case OrderProcessStatusEnum.ItemCodeNotExist: //ItemCode not found
                            request = new RequestToCallback
                            {
                                Code = orderResult.Result.Status.ApiCode(),
                                Message = $"{orderResult.Result.Message} - {orderResult.ErrorId}",
                                POSOrderNo = "",
                                OrderId = orderResult.TTOrderId
                            };
                            break;
                        case OrderProcessStatusEnum.PrinterServerNoResponce:
                            request = new RequestToCallback
                            {
                                Code = orderResult.Result.Status.ApiCode(),
                                Message = $"{orderResult.Result.Message} - {orderResult.ErrorId}",
                                POSOrderNo = orderResult.PosOrderNo,
                                OrderId = orderResult.TTOrderId
                            };
                            break;
                        case OrderProcessStatusEnum.FailedToGetOrderNo: //FailedToGetOrderNo
                            request = new RequestToCallback
                            {
                                Code = orderResult.Result.Status.ApiCode(),
                                Message = $"{orderResult.Result.Message} - {orderResult.ErrorId}",
                                POSOrderNo = "",
                                OrderId = orderResult.TTOrderId
                            };
                            break;
                        default:
                            request = new RequestToCallback
                            {
                                Code = 3000,
                                Message = "Unknown Error.",
                                POSOrderNo = "",
                                OrderId = orderResult.TTOrderId
                            };
                            break;
                    }

                   
                    HttpResponseMessage response = await _client.PostAsJsonAsync($"{this._options.T_CallBackBaseUrl}confirmorder", request);

                    string responseString = await response.Content.ReadAsStringAsync();

                    ResponseFromCallback data = JsonConvert.DeserializeObject<ResponseFromCallback>(responseString);
                    if (response.IsSuccessStatusCode)
                    {
                        if (data.Code == 0)
                        {
                            this._logger.LogInformation($"Confirm Order Status callback of {orderResult.TTOrderId} - {orderResult.PosOrderNo} is successful.");
                        }
                        else
                        {
                            this._logger.LogError($"Error. Callback of {orderResult.TTOrderId} - {orderResult.PosOrderNo} is failed. Reason: {data.Message}");
                        }

                    }
                    else
                    {
                        this._logger.LogError($"Error. Callback of {orderResult.TTOrderId} - {orderResult.PosOrderNo} is failed. Reason: {response.ReasonPhrase}");
                    }
                }


            }
            catch (Exception ex) {
                this._logger.LogError($"Error. Callback of {orderResult.TTOrderId} - {orderResult.PosOrderNo} is failed. Reason: {ex.Message}");
            }
           

        }

        public static string GetDDAOrderPickupNo(string orderNo)
        {
            var pickNoStr = orderNo.Substring(orderNo.Length - 3);

            int pickNo = 1;
            int.TryParse(pickNoStr, out pickNo);
            var pick_No = "";
            var modLeft = pickNo % 100;
            if (modLeft == 0)
            {
                pick_No = $"100";
            }
            else
            {

                pick_No = $"000{pickNo}";
                pick_No = pick_No.Substring(pick_No.Length - 3);
            }

            return pick_No;
        }

    }

        public class ResponseFromCallback {
            public int Code { get; set; }  //Code = 0, success; Code = 2001, failed;
            public string Message { get; set; }
        }

        public class RequestToCallback
        {
            public int Code { get; set; }
            public string Message { get; set; }
            public string POSOrderNo { get; set; }
            public int OrderId { get; set; }
        }


}
