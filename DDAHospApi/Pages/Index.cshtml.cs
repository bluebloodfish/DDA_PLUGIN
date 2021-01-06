using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DDAApi.HospModel;
using DDAApi.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DDAApi.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHospMenuManager _dbManager;

        public IndexModel(IHospMenuManager manager)
        {
            this._dbManager = manager;
        }


        public void OnGet()
        {
            List<MenuItem> menuitems = this._dbManager.GetMenuItems();
            if (menuitems.Count > 0) {
                foreach (var mi in menuitems) {
                    Console.WriteLine(mi.Description1);
                }
            }
            

        }
    }
}
