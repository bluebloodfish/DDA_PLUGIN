using DDAApi.WebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.OrderQueue
{
    public interface IOrderQueueService
    {
        void Enqueue(PlatformOrder order);
    }
}
