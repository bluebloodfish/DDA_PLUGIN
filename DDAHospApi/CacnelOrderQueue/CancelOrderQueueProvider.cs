using DDAApi.WebApi.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.CancelOrderQueue
{
    public class CancelOrderQueueProvider : ICancelOrderQueueProvider
    {
        private static readonly ConcurrentQueue<string> _cancelOrderQueue = new ConcurrentQueue<string>();

        public int Count => _cancelOrderQueue.Count;

        public bool IsEmpty => _cancelOrderQueue.IsEmpty;

        public void Enqueue(string ttorderid)
        {
            _cancelOrderQueue.Enqueue(ttorderid);
        }

        public bool TryDequeue(out string orderid)
        {
            return _cancelOrderQueue.TryDequeue(out orderid);
        }

    }
}
