﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.DataAccess
{
    public interface IHospTableManage
    {
        bool IsTableAvaliable(string tableNo);
    }
}
