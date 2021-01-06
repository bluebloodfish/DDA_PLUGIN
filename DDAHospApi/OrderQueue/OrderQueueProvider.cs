using DDAApi.DataAccess;
using DDAApi.WebApi.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.OrderQueue
{
    public class OrderQueueProvider: IOrderQueueProvider
    {
        private static readonly ConcurrentQueue<PlatformOrder> _orderQueue = new ConcurrentQueue<PlatformOrder>();

       

        public int Count => _orderQueue.Count;

        public bool IsEmpty => _orderQueue.IsEmpty;

        public void Enqueue(PlatformOrder order)
        {
            _orderQueue.Enqueue(order);
        }

        public bool TryDequeue(out PlatformOrder order)
        {
            return _orderQueue.TryDequeue(out order);
        }

        
    }
}
