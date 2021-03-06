﻿using DDAApi.HospModel;
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


        IEnumerable<TResult> GetMenuItems<TResult>(Func<MenuItem, TResult> selector, int Page_Index);
        int GetMenuItemTotalRows();

        IEnumerable<TResult> GetCategories<TResult>(Func<Category, TResult> selector);
        IEnumerable<TResult> GetCategories<TResult>(Func<Category, TResult> selector, int Page_Index);
        int GetCategoryTotalRows();

        Task<List<MenuItem>> GetMenuItemsForMTAsync(int Page_Index);
        Task<List<Category>> GetCategoryForMTAsync(int Page_Index);
    }
}
