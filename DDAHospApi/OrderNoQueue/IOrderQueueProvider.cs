using DDAApi.WebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.OrderNoQueue
{
    public interface IOrderNoQueueProvider
    {
        //void Enqueue(OrderNoOption orderNo);
        //bool TryDequeue(out OrderNoOption orderNo);
        int Count { get; }
        bool IsEmpty { get; }
        void ChangeSeed(OrderNoParts parts);
        string GetNewOrderNo();
        OrderNoParts InitOrderNoParts(DateTime now);
    }
}
