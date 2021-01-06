using DDAApi.WebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.OrderQueue
{
    public class OrderQueueService: IOrderQueueService
    {
        private readonly IOrderQueueProvider _provider;

        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="provider"></param>
        public OrderQueueService(IOrderQueueProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="box"></param>
        public void Enqueue(PlatformOrder order)
        {
            _provider.Enqueue(order);
        }
    }
}
