using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DDAApi.HospModel;
using DDAApi.Security;
using DDAApi.DataAccess;
using DDAApi.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using DDAApi.WebApi.Model;

namespace DDAApi.WebApi
{
    [Route("api_v1/[controller]")]
    [ApiController]
    public class HospMenuController : ControllerBase
    {
        public ILogger<HospMenuController> _logger { get; }
        private readonly IHospMenuManager _dbManager;
        private readonly IHospTableManage _tableManage;
        private readonly IConfiguration _config;
        private readonly IHospOrderManage _orderManage;

        public HospMenuController(ILogger<HospMenuController> logger, IHospMenuManager dbManager, IConfiguration config, IHospOrderManage orderManage, IHospTableManage tableManage)
        {
            this._logger = logger;
            this._dbManager = dbManager;
            this._config = config;
            this._orderManage = orderManage;
            this._tableManage = tableManage;
        }


        [ServiceFilter(typeof(AuthenFilter))]
        [HttpGet("GetCategories")]
        public IActionResult GetCategories()
        {
            var cnDesc_No_Str = this._config["Item_CN"];
            var enDesc_No_Str = this._config["Item_EN"];

            int cnDesc_No, enDesc_No;
            int.TryParse(cnDesc_No_Str, out cnDesc_No);
            int.TryParse(enDesc_No_Str, out enDesc_No);

            List<Category> categories = this._dbManager.GetCategories();
            categories = categories.OrderBy(x => x.Code).ToList();

            if (categories.Count <= 0)
            {
                return NotFound(new { Result = 0, Data = "", Message = "No Categories found" });
            }


            List<E_Category> eCategories = new List<E_Category>();

            for (int i = 0; i < categories.Count; i++)
            {
                E_Category e_Category = new E_Category();


                switch (cnDesc_No)
                {
                    case 1:
                        e_Category.Name_CN = categories[i].Name1;
                        break;
                    case 2:
                        e_Category.Name_CN = string.IsNullOrEmpty(categories[i].Name2) ? categories[i].Name1 : categories[i].Name2;
                        break;
                    case 3:
                        e_Category.Name_CN = string.IsNullOrEmpty(categories[i].Name3) ? categories[i].Name1 : categories[i].Name3;
                        break;
                    case 4:
                        e_Category.Name_CN = string.IsNullOrEmpty(categories[i].Name4) ? categories[i].Name1 : categories[i].Name4;
                        break;
                    default:
                        break;
                }

                switch (enDesc_No)
                {
                    case 1:
                        e_Category.Name_EN = categories[i].Name1 ;
                        break;
                    case 2:
                        e_Category.Name_EN = string.IsNullOrEmpty(categories[i].Name2) ? categories[i].Name1 : categories[i].Name2;
                        break;
                    case 3:
                        e_Category.Name_EN = string.IsNullOrEmpty(categories[i].Name3) ? categories[i].Name1 : categories[i].Name3;
                        break;
                    case 4:
                        e_Category.Name_EN = string.IsNullOrEmpty(categories[i].Name4) ? categories[i].Name1 : categories[i].Name4;
                        break;
                    default:
                        break;
                }

                e_Category.Code = categories[i].Code;
                e_Category.Enable = categories[i].Enable;
                e_Category.Position = i + 1;

                eCategories.Add(e_Category);
            }

            
            return Ok(new {Result = 1, Data = eCategories, Message = "OK" });
           
        }


        [ServiceFilter(typeof(AuthenFilter))]
        [HttpGet("GetMenuItems")]
        public IActionResult GetMenuItems()
        {
            var cnDesc_No_Str = this._config["Item_CN"];
            var enDesc_No_Str = this._config["Item_EN"];

            int cnDesc_No, enDesc_No;
            int.TryParse(cnDesc_No_Str, out cnDesc_No);
            int.TryParse(enDesc_No_Str, out enDesc_No);

            List<MenuItem> menuItems = this._dbManager.GetMenuItems();
            if (menuItems.Count <= 0)
            {
                return NotFound(new { Result = 0, Data = "", Message = "No MenuItem found!!" });
            }

            List<E_MenuItem> eMenuItemList = new List<E_MenuItem>();

            foreach (var mItem in menuItems) {
                E_MenuItem eMenuItem = new E_MenuItem();
                switch (cnDesc_No) {
                    case 1:
                        eMenuItem.Name_CN = mItem.Description1;
                        break;
                    case 2:
                        eMenuItem.Name_CN = string.IsNullOrEmpty(mItem.Description2) ? mItem.Description1 : mItem.Description2;
                        break;
                    case 3:
                        eMenuItem.Name_CN = string.IsNullOrEmpty(mItem.Description3) ? mItem.Description1 : mItem.Description3;
                        break;
                    case 4:
                        eMenuItem.Name_CN = string.IsNullOrEmpty(mItem.Description4) ? mItem.Description1 : mItem.Description4;
                        break;
                    default:
                        break;
                }

                switch (enDesc_No)
                {
                    case 1:
                        eMenuItem.Name_EN = mItem.Description1;
                        break;
                    case 2:
                        eMenuItem.Name_EN = string.IsNullOrEmpty(mItem.Description2) ? mItem.Description1 : mItem.Description2;
                        break;
                    case 3:
                        eMenuItem.Name_EN = string.IsNullOrEmpty(mItem.Description3) ? mItem.Description1 : mItem.Description3;
                        break;
                    case 4:
                        eMenuItem.Name_EN = string.IsNullOrEmpty(mItem.Description4) ? mItem.Description1 : mItem.Description4;
                        break;
                    default:
                        break;
                }

                eMenuItem.ItemCode = mItem.ItemCode;
                eMenuItem.Category = mItem.Category;
                eMenuItem.TaxRate =mItem.TaxRate;
                eMenuItem.Price1 = mItem.Price1;
                eMenuItem.Price2 = mItem.Price2;
                eMenuItem.Price3 = mItem.Price3;
                eMenuItem.Price4 = mItem.Price4;

                eMenuItem.SubDec1 = mItem.SubDescription1;
                eMenuItem.SubDec2 = mItem.SubDescription2;
                eMenuItem.SubDec3 = mItem.SubDescription3;
                eMenuItem.SubDec4 = mItem.SubDescription4;

                eMenuItem.SpecialPice1 = (int)(mItem.HappyHourPrice1 * 100);
                eMenuItem.SpecialPice2 = (int)(mItem.HappyHourPrice2 * 100);
                eMenuItem.SpecialPice3 = (int)(mItem.HappyHourPrice3 * 100);
                eMenuItem.SpecialPice4 = (int)(mItem.HappyHourPrice4 * 100);

                eMenuItem.Position = mItem.MainPosition;

                eMenuItemList.Add(eMenuItem);
            }

            
           
         return Ok(new { Result = 1, Data = eMenuItemList, Message = "OK" });
           
            
        }

        [ServiceFilter(typeof(AuthenFilter))]
        [HttpGet("GetMenuItems/{categoryCode}")]
        public IActionResult GetMenuItems(string categoryCode)
        {

            var cnDesc_No_Str = this._config["Item_CN"];
            var enDesc_No_Str = this._config["Item_EN"];

            int cnDesc_No, enDesc_No;
            int.TryParse(cnDesc_No_Str, out cnDesc_No);
            int.TryParse(enDesc_No_Str, out enDesc_No);

            List<MenuItem> menuItems = this._dbManager.GetMenuItems(categoryCode);
            if (menuItems.Count <= 0)
            {
                return NotFound(new { Result = 0, Data = "", Message = "No Menu Item found!!" });
            }

            List<E_MenuItem> eMenuItemList = new List<E_MenuItem>();

            foreach (var mItem in menuItems)
            {
                E_MenuItem eMenuItem = new E_MenuItem();
                switch (cnDesc_No)
                {
                    case 1:
                        eMenuItem.Name_CN = mItem.Description1;
                        break;
                    case 2:
                        eMenuItem.Name_CN = string.IsNullOrEmpty(mItem.Description2) ? mItem.Description1 : mItem.Description2;
                        break;
                    case 3:
                        eMenuItem.Name_CN = string.IsNullOrEmpty(mItem.Description3) ? mItem.Description1 : mItem.Description3;
                        break;
                    case 4:
                        eMenuItem.Name_CN = string.IsNullOrEmpty(mItem.Description4) ? mItem.Description1 : mItem.Description4;
                        break;
                    default:
                        break;
                }

                switch (enDesc_No)
                {
                    case 1:
                        eMenuItem.Name_EN = mItem.Description1;
                        break;
                    case 2:
                        eMenuItem.Name_EN = string.IsNullOrEmpty(mItem.Description2) ? mItem.Description1 : mItem.Description2;
                        break;
                    case 3:
                        eMenuItem.Name_EN = string.IsNullOrEmpty(mItem.Description3) ? mItem.Description1 : mItem.Description3;
                        break;
                    case 4:
                        eMenuItem.Name_EN = string.IsNullOrEmpty(mItem.Description4) ? mItem.Description1 : mItem.Description4;
                        break;
                    default:
                        break;
                }

                eMenuItem.ItemCode = mItem.ItemCode;
                eMenuItem.Category = mItem.Category;
                eMenuItem.TaxRate = mItem.TaxRate;
                eMenuItem.Price1 = mItem.Price1;
                eMenuItem.Price2 = mItem.Price2;
                eMenuItem.Price3 = mItem.Price3;
                eMenuItem.Price4 = mItem.Price4;

                eMenuItem.SubDec1 = mItem.SubDescription1;
                eMenuItem.SubDec2 = mItem.SubDescription2;
                eMenuItem.SubDec3 = mItem.SubDescription3;
                eMenuItem.SubDec4 = mItem.SubDescription4;

                eMenuItem.SpecialPice1 = mItem.HappyHourPrice1;
                eMenuItem.SpecialPice2 = mItem.HappyHourPrice2;
                eMenuItem.SpecialPice3 = mItem.HappyHourPrice3;
                eMenuItem.SpecialPice4 = mItem.HappyHourPrice4;

                eMenuItem.Position = mItem.MainPosition;

                eMenuItemList.Add(eMenuItem);
            }

            return Ok(new { Result = 1, Data = eMenuItemList, Message = "OK" });
        }
    }

    #region Out Data
    public class E_MenuItem
    {
        public string ItemCode { get; set; }

        public string Name_CN { get; set; }
        public string Name_EN { get; set; }

        public string Category { get; set; }

        public double TaxRate { get; set; }

        public double Price1 { get; set; }
        public double Price2 { get; set; }
        public double Price3 { get; set; }
        public double Price4 { get; set; }

        public string SubDec1 { get; set; }
        public string SubDec2 { get; set; }
        public string SubDec3 { get; set; }
        public string SubDec4 { get; set; }

        public double SpecialPice1 { get; set; }
        public double SpecialPice2 { get; set; }
        public double SpecialPice3 { get; set; }
        public double SpecialPice4 { get; set; }
        public int Position { get; set; }

    }


    public class E_Category
    {
        public string Code { get; set; }
        public string Name_CN { get; set; }
        public string Name_EN { get; set; }
        public bool Enable { get; set; }
        public int Position { get; set; }

    }

    #endregion
}