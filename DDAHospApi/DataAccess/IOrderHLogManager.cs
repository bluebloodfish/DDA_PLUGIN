using DDAApi.HospModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.DataAccess
{
    public interface IOrderHLogManager
    {
        Task<int> UpdateOrderHLogStatus(string ttOrderId, int status);
        IEnumerable<string> GetPendingOrderHLog();
        TT_OrderH_Log GetOrderHLog(string orderId);
        Task<int> UpdateOrderHLog(TT_OrderH_Log orderHLog);
        IEnumerable<string> GetOrderHLogs(string orderNo);
    }
}
