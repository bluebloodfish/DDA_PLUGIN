using DDAApi.Utility;
using DDAApi.WebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.OrderQueue
{
    public interface IOrderProcessor
    {
        Task<OrderProcessResult> SimpleOrder(PlatformOrder pOrder);
        Task<OrderProcessResult> POSCodeOrder(PlatformOrder pOrder);
    }
}
