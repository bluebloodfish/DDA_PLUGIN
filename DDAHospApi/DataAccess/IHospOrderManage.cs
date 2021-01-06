using DDAApi.HospModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.DataAccess
{
    public interface IHospOrderManage
    {
        string GetNewOrderNo();
        Task<int> SaveOrder(HospOrder order);
        Task<int> MergeOrder(HospOrder order);
        HospOrderHead GetHospOrderHead(string orderNo);
        Task<int> CreateOrderItems(List<HospOrderItem> items);
        Task<int> UpdateOrderItems(List<HospOrderItem> items);
        Task<int> CreateRecvAcct(HospRecvAcct recvAcct);
        Task<int> MarkVoidOrderItems(string orderNo);
        Task UpdateNotesfor8287(string notes, string orderNo);

        HospOrder GetOccupiedTalbeOrder(string tableNo);

    }
}
