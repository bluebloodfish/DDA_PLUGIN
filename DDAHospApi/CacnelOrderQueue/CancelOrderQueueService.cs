using DDAApi.WebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.CancelOrderQueue
{
    public class CancelOrderQueueService : ICancelOrderQueueService
    {
        private readonly ICancelOrderQueueProvider _provider;

        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="provider"></param>
        public CancelOrderQueueService(ICancelOrderQueueProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="box"></param>
        public void Enqueue(string orderid)
        {
            _provider.Enqueue(orderid);
        }
    }
}
