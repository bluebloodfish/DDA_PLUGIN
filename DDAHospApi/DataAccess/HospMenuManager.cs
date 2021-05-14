using DDAApi.DataAccess;
using DDAApi.HospModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.DataAccess
{
    public class HospMenuManager: IHospMenuManager
    {
        private readonly AppDbContext _ctx;
        private readonly IConfiguration _config;

        public HospMenuManager(AppDbContext ctx, IConfiguration config)
        {
            this._ctx = ctx;
            this._config = config;
        }

        //public List<Category> GetCategories() {
        //    var MenuTypeSetting = this._config["MenuType"];
        //    int.TryParse(MenuTypeSetting, out int MenuType);

        //    switch (MenuType) {
        //        case 1:
        //            return this._ctx.Catregories.Where(x => x.ShowOnMainMenu == true).ToList();
        //        case 2:
        //            return this._ctx.Catregories.Where(x => x.ShowOnPOSMenu == true).ToList();
        //        case 3:
        //            return this._ctx.Catregories.Where(x => x.ShowOnPhoneOrderMenu == true).ToList();
        //        default:
        //            return this._ctx.Catregories.Where(x => x.ShowOnMainMenu == true).ToList();
        //    }
        //}


        public IEnumerable<TResult> GetCategories<TResult>(Func<Category, TResult> selector)
        {
            return this._ctx.Catregories.Select(selector);
        }

        //public CategoryPaginationResult GetCategories(int Page_Index)
        //{
        //    this._ctx.Catregories.Select();

        //}

        public List<Category> GetCategories()
        {
            return this._ctx.Catregories.ToList();
        }

        public IEnumerable<TResult> GetCategories<TResult>(Func<Category, TResult> selector, int Page_Index)
        {
            return this._ctx.Catregories.OrderBy(x => x.Code).Skip(50 * Page_Index).Take(50)                
              .Select(selector);
        }

        public int GetCategoryTotalRows()
        {
            return this._ctx.Catregories.Count();
        }

        public IEnumerable<TResult> GetMenuItems<TResult>(Func<MenuItem, TResult> selector, int Page_Index)
        {
            return this._ctx.MenuItems.OrderBy(x => x.ItemCode).Skip(50 * Page_Index).Take(50)
              .Select(selector);
        }

        public int GetMenuItemTotalRows()
        {
            return this._ctx.MenuItems.Count();
        }


        public List<MenuItem> GetMenuItems()
        {
            return this._ctx.MenuItems.ToList();
        }

        public List<MenuItem> GetMenuItems(string categoryCode)
        {
            var category = GetCategories().Where(x => x.Code.ToLower() == categoryCode.ToLower()).FirstOrDefault();
            if (category == null)
            {
                return new List<MenuItem>();
            }
            else
            {
                return this._ctx.MenuItems.Where(x => x.Category.ToLower() == category.Name1.ToLower()
                                                ).ToList();
            }
            
        }

        public bool IsMainItem(string itemCode) {
            var item = this._ctx.MenuItems.Where(x => x.ItemCode == itemCode).FirstOrDefault();

            if (item == null) {
                return false;
            }
            else {
                return !item.Instruction;
            }
        }



        public async Task<List<MenuItem>> GetMenuItemsForMTAsync(int Page_Index)
        {
            var menuItems = await this._ctx.MenuItems.FromSql($"sp_GetMenuItemForMt {Page_Index}").ToListAsync();
            return menuItems;

        }

        public async Task<List<Category>> GetCategoryForMTAsync(int Page_Index)
        {
            var Catregories = await this._ctx.Catregories.FromSql($"sp_GetCategoryForMt {Page_Index}").ToListAsync();
            return Catregories;

        }

    }

    public class CategoryPaginationResult {
        public int Total_Row { get; set; }
        public List<Category>  Categories { get; set; }
    }
}
