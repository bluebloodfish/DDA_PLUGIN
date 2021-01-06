using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.DataAccess
{
    public interface ISqAppSettings
    {
        Task<int> AddSqHttpCallback(string baseUrl);
        int GetOnlineOrderStartYear();
        Task<int> SetOnlineOrderStartYear(int year);
    }
}

