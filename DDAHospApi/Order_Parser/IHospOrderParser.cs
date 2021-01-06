using DDAApi.HospModel;
using DDAApi.WebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.Order_Parser
{
    public interface IHospOrderParser
    {
        //HospOrder CompleteHospOrder(PlatformOrder pOrder);
        OrderParserResult HospOrderWPOSCode(PlatformOrder pOrder);
        OrderParserResult SimpleHospOrder(PlatformOrder pOrder);

        OrderParserResult HospMergeOrderWPOSCode(PlatformOrder pOrder, HospOrder orgHospOrder);
    }
}
