using DDAApi.HospModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.DataAccess
{
    public interface ITT_OrderProcess_Log_Manage
    {
        Task<int> AddOrderProcessLog(TT_OrderProcess_Log log);
    }
}
