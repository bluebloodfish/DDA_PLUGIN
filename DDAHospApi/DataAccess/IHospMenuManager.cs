using DDAApi.HospModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.DataAccess
{
    public interface IHospMenuManager
    {
        List<Category> GetCategories();
        List<MenuItem> GetMenuItems();
        List<MenuItem> GetMenuItems(string category);
        bool IsMainItem(string itemCode);
    }
}
