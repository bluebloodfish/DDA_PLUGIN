using DDAApi.WebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.CancelOrderQueue
{
    public interface ICancelOrderQueueProvider
    {
        void Enqueue(string orderid);
        bool TryDequeue(out string orderid);
        int Count { get; }
        bool IsEmpty { get; }
    }
}
