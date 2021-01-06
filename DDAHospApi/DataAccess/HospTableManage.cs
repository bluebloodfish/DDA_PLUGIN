using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DDAApi.HospModel;
using DDAApi.Utility;
using Microsoft.Extensions.Options;

namespace DDAApi.DataAccess
{
    public class HospTableManage : IHospTableManage
    {
        private readonly AppDbContext _ctx;
       
        public HospTableManage(AppDbContext ctx)
        {
            this._ctx = ctx;
        }

        public bool IsTableAvaliable(string tableNo)
        {
            var occupiedTables = this._ctx.HospOrderHeads.Where(x => x.OrderDate.Date.Equals(DateTime.Now.Date) && x.BillKind == 0 && !x.Credit)
                .Select(x => x.TableNo).ToList();
           

            if (occupiedTables == null || occupiedTables.Count() == 0)
            {
                return true;
            }
            else {
                if (occupiedTables.Any(x => x.ToLower() == tableNo.ToLower()))
                {
                    return false;
                }
                else {
                    return true;
                }
            }
        }

        
    }
}
