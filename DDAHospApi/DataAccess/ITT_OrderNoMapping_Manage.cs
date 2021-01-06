using DDAApi.HospModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.DataAccess
{
    public interface ITT_OrderNoMapping_Manage
    {
        Task<int> AddOrderNoMapping(TT_OrderNoMapping mapping);

        TT_OrderNoMapping GetMapping(string orderNo, int ttId);
    }
}
