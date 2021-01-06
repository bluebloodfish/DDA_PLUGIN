using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DDAApi.HospModel;
using DDAApi.Utility;
using Microsoft.Extensions.Options;

namespace DDAApi.DataAccess
{
    public class TT_OrderProcess_Log_Manage : ITT_OrderProcess_Log_Manage
    {
        private readonly AppDbContext _ctx;
       
        public TT_OrderProcess_Log_Manage(AppDbContext ctx)
        {
            this._ctx = ctx;
        }

        public Task<int> AddOrderProcessLog(TT_OrderProcess_Log log)
        {
            this._ctx.TT_OrderProcess_Logs.Add(log);
            return this._ctx.SaveChangesAsync();
        }

        
    }
}
