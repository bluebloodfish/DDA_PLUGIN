using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DDAApi.HospModel;
using DDAApi.Utility;
using Microsoft.Extensions.Options;

namespace DDAApi.DataAccess
{
    public class TT_OrderNoMapping_Manage : ITT_OrderNoMapping_Manage
    {
        private readonly AppDbContext _ctx;
       
        public TT_OrderNoMapping_Manage(AppDbContext ctx)
        {
            this._ctx = ctx;
        }

        public Task<int> AddOrderNoMapping(TT_OrderNoMapping mapping) {
            this._ctx.TT_OrderNoMappings.Add(mapping);
            return this._ctx.SaveChangesAsync();
        }

        public TT_OrderNoMapping GetMapping(string orderNo, int ttId) {
            var mapping = this._ctx.TT_OrderNoMappings.Where(x => x.OrderNo == orderNo && x.TTId == ttId).FirstOrDefault();
            return mapping;
        }

    }
}
