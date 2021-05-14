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
    public class MenuForMTController : ControllerBase
    {
        public ILogger<MenuForMTController> _logger { get; }
        private readonly IHospMenuManager _menuManager;
        private readonly IHospTableManage _tableManage;
        private readonly IConfiguration _config;
        private readonly IHospOrderManage _orderManage;

        public MenuForMTController(ILogger<MenuForMTController> logger, IHospMenuManager menuManager, IConfiguration config, IHospOrderManage orderManage, IHospTableManage tableManage)
        {
            this._logger = logger;
            this._menuManager = menuManager;
            this._config = config;
            this._orderManage = orderManage;
            this._tableManage = tableManage;
        }

        [HttpGet("GetVersion")]
        public IActionResult GetVersion()
        {
            return Ok(new { code = 0, data = new { Version = "1.0.3" } });
        }

        [ServiceFilter(typeof(AuthenFilter))]
        [HttpPost("GetMenuItems")]
        public async Task<IActionResult> GetMenuItems([FromBody]MT_Menu_Request request)
        {
            var json = JsonConvert.SerializeObject(request);
            this._logger.LogInformation($"*********GetMenuItems******** \n {json} \n***************************");

            try
            {
                var total = this._menuManager.GetMenuItemTotalRows();

                var total_pages = (int)Math.Ceiling(total / (double)50);

                if (request.Page_Index >= total_pages)
                {
                    return Ok(new { code = 1001, message = $"Maximum Page_Index is {total_pages - 1}" });

                }
                var categoryList = this._menuManager.GetCategories(x => x).ToList();

                var menuItemList = await this._menuManager.GetMenuItemsForMTAsync(request.Page_Index);

                var list = new List<MT_MenuItem_Response>();

                foreach (var x in menuItemList) {
                    var item = new MT_MenuItem_Response();
                    item.ItemCode = x.ItemCode;
                    item.Description1 = x.Description1;
                    item.Description2 = string.IsNullOrEmpty(x.Description2) ? "" : x.Description2;
                    item.Description3 = string.IsNullOrEmpty(x.Description3) ? "" : x.Description3;
                    item.Description4 = string.IsNullOrEmpty(x.Description4) ? "" : x.Description4;
                    item.Category = x.Category;

                    //if (item.ItemCode.ToUpper() == "NM28") {
                    //    this._logger.LogError($"ItemCode={item.ItemCode}, Price1={x.Price1}, Price2={x.Price2}, Price3={x.Price3}, Price4={x.Price4}");
                    //}

                    item.Price  = (int)(Math.Round(x.Price1 * 100, 2));
                    item.Price1 = (int)(Math.Round(x.Price2 * 100, 2));
                    item.Price2 = (int)(Math.Round(x.Price3 * 100, 2));
                    item.Price3 = (int)(Math.Round(x.Price4 * 100, 2));

                    item.SubDescription = string.IsNullOrEmpty(x.SubDescription1) ? "" : x.SubDescription1;
                    item.SubDescription1 = string.IsNullOrEmpty(x.SubDescription2) ? "" : x.SubDescription2;
                    item.SubDescription2 = string.IsNullOrEmpty(x.SubDescription3) ? "" : x.SubDescription3;
                    item.SubDescription3 = string.IsNullOrEmpty(x.SubDescription4) ? "" : x.SubDescription4;
                    item.TaxRate = (int)x.TaxRate;
                    item.Instruction = x.Instruction;
                    item.Multiple = x.Multiple;
                    item.Active = x.Active;
                    list.Add(item);
                }


                foreach (var cate in categoryList)
                {
                    foreach (var item in list)
                    {

                        if (item.Category.ToUpper() == cate.Name1.ToUpper())
                        {
                            item.CategoryCode = cate.Code;
                        }
                    }
                }

                return Ok(new { code = 0, data = new { Total_Rows = total, Total_Pages = total_pages, Page_Size = list.Count(), Rows = list }, message = "Ok" });


            }
            catch (Exception e)
            {
                var errId = TokenFactory.GenerateErrorId();
                this._logger.LogError($"ErrorId: {errId}");
                this._logger.LogError(e.Message);
                return Ok(new { code = 2000, message = $"{errId} - {e.Message}", error_id = errId });
            }

        }



        [ServiceFilter(typeof(AuthenFilter))]
        [HttpPost("GetCategories")]
        public async Task<IActionResult> GetCategories([FromBody]MT_Menu_Request request)
        {
            var json = JsonConvert.SerializeObject(request);
            this._logger.LogInformation($"*********GetCategories******** \n {json} \n***************************");

            try
            {
                var total = this._menuManager.GetCategoryTotalRows();

                var total_pages = (int)Math.Ceiling(total / ((double)50));
               

                if (request.Page_Index >= total_pages)
                {
                    return Ok(new { code = 1001, message = $"Maximum Page_Index is {total_pages - 1}" });

                }

                var categoryList = await this._menuManager.GetCategoryForMTAsync(request.Page_Index);

                var list = new List<MT_Category_Response>();
                foreach(var x in categoryList) {
                    var item = new MT_Category_Response();
                    item.Code = x.Code;
                    item.Active = x.Enable;
                    item.Name1 = x.Name1;
                    item.Name2 = string.IsNullOrEmpty(x.Name2) ? "" : x.Name2;
                    list.Add(item);
                }

                return Ok(new { code = 0, data = new { Total_Rows = total, Total_Pages = total_pages, Page_Size = list.Count(), Rows = list }, message = "Ok" });
            }
            catch (Exception e) {
                var errId = TokenFactory.GenerateErrorId();
                this._logger.LogError($"ErrorId: {errId}");
                this._logger.LogError(e.Message);
                return Ok(new { code = 2000, message =$"{errId} - {e.Message}", error_id = errId });
            }



        }


        public class MT_Menu_Request {
            public int Page_Index { get; set; }
        }



        public class MT_MenuItem_Response {
            public string ItemCode { get; set; }
            public string Description1 { get; set; }
            public string Description2 { get; set; }
            public string Description3 { get; set; }
            public string Description4 { get; set; }

            public string CategoryCode { get; set; }
            public string Category { get; set; }
            public int Price { get; set; }
            public int Price1 { get; set; }
            public int Price2 { get; set; }
            public int Price3 { get; set; }
            
            public string SubDescription { get; set; }
            public string SubDescription1 { get; set; }
            public string SubDescription2{ get; set; }
            public string SubDescription3 { get; set; }


            public int TaxRate { get; set; }
            public bool Instruction { get; set; }
            public bool Multiple { get; set; }
            public bool Active { get; set; }

        }

        public class MT_Category_Response
        {
            public string Code { get; set; }
            public string Name1 { get; set; }
            public string Name2 { get; set; }

            public bool Active { get; set; }

        }
    }

}