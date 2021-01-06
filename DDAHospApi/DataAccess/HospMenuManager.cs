using DDAApi.DataAccess;
using DDAApi.HospModel;
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

        public List<Category> GetCategories() {
            var MenuTypeSetting = this._config["MenuType"];
            int.TryParse(MenuTypeSetting, out int MenuType);

            switch (MenuType) {
                case 1:
                    return this._ctx.Catregories.Where(x => x.ShowOnMainMenu == true).ToList();
                case 2:
                    return this._ctx.Catregories.Where(x => x.ShowOnPOSMenu == true).ToList();
                case 3:
                    return this._ctx.Catregories.Where(x => x.ShowOnPhoneOrderMenu == true).ToList();
                default:
                    return this._ctx.Catregories.Where(x => x.ShowOnMainMenu == true).ToList();
            }
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

    }
}
