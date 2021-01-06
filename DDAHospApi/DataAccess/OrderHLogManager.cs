using DDAApi.HospModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.DataAccess
{
    public class OrderHLogManager: IOrderHLogManager
    {
        private readonly AppDbContext _ctx;

        public OrderHLogManager(AppDbContext ctx)
        {
            this._ctx = ctx;
        }

        public async Task<int> UpdateOrderHLogStatus(string ttOrderId, int status)
        {
            var log = this._ctx.TT_OrderH_Logs.Where(x => x.TTOrderId == ttOrderId).FirstOrDefault();
            log.Status = status;
            log.ModifiedDateTime = DateTime.Now;
            _ctx.TT_OrderH_Logs.Update(log);
            return await this._ctx.SaveChangesAsync();
        }

        public async Task<int> UpdateOrderHLog(TT_OrderH_Log orderHLog)
        {
            var log = this._ctx.TT_OrderH_Logs.Where(x => x.TTOrderId == orderHLog.TTOrderId).FirstOrDefault();
            log.Retried = orderHLog.Retried;
            log.Status = orderHLog.Status;
            log.Message = orderHLog.Message;
            log.ModifiedDateTime = DateTime.Now;
            _ctx.TT_OrderH_Logs.Update(log);
            return await this._ctx.SaveChangesAsync();
        }


        public IEnumerable<string> GetPendingOrderHLog()
        {
            return this._ctx.TT_OrderH_Logs.Where(x => x.Status == OrderHLogStatus.Pending.Code).Select(x=> x.TTOrderId);
           
        }

        public IEnumerable<string> GetOrderHLogs(string orderNo)
        {
            return this._ctx.TT_OrderH_Logs.Where(x => x.OrderNo == orderNo).Select(x => x.TTOrderId);

        }

        public TT_OrderH_Log GetOrderHLog(string orderId)
        {
            return this._ctx.TT_OrderH_Logs.Where(x => x.TTOrderId == orderId).FirstOrDefault();

        }

    }

    public static class OrderHLogStatus {
        public readonly static StatusInfo Failed = new StatusInfo { Code = -1, Name = "failed" };
        public readonly static StatusInfo Pending = new StatusInfo { Code = 0, Name = "pending" };
        public readonly static StatusInfo Success = new StatusInfo { Code = 1, Name = "success" };


    }

    public class StatusInfo
    {
        public int Code { get; set; }
        public string Name { get; set; }
    }
}
