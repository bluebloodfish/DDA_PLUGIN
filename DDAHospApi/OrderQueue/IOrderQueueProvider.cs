using DDAApi.WebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.OrderQueue
{
    public interface IOrderQueueProvider
    {
        void Enqueue(PlatformOrder order);
        bool TryDequeue(out PlatformOrder order);
        int Count { get; }
        bool IsEmpty { get; }
    }
}
