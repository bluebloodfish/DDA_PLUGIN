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
using DDAApi.OrderQueue;
using Microsoft.Extensions.Options;
using DDAApi.Order_Parser;
using Microsoft.Extensions.DependencyInjection;
using DDAApi.TTOpenApi;
using Microsoft.EntityFrameworkCore;
using DDAApi.CancelOrderQueue;
using DDAApi.OrderNoQueue;

namespace DDAApi.WebApi
{
    [Route("api_v1/[controller]")]
    [ApiController]
    public class ConsoleController : ControllerBase
    {
        public ILogger<ConsoleController> _logger { get; }

        

        public ConsoleController(ILogger<ConsoleController> logger)
        {
            this._logger = logger;
            
        }

        [HttpGet("GetVersion")]
        public IActionResult GetVersion()
        {
            return Ok(new { code = 0, data = new { Version = "3.3.11" } });
        }

    }
}