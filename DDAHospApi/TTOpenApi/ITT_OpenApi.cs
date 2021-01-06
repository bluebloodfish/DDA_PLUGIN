using DDAApi.OrderQueue;
using DDAApi.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.TTOpenApi
{
    public interface ITT_OpenApi
    {
        Task CallbackToConfirmOrder(OrderProcessResult orderResult);
    }
}
