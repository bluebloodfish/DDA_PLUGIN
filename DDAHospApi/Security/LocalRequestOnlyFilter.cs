using DDAApi.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.Security
{
    public class LocalRequestOnlyFilter : IAsyncActionFilter
    {
       
        private readonly ILogger<LocalRequestOnlyFilter> _logger;
        
        public LocalRequestOnlyFilter(ILogger<LocalRequestOnlyFilter> logger, IMemoryCache memoryCache, IConfiguration  config, IOptions<DDAApiSetting> options)
        {
            this._logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var connection = context.HttpContext.Connection;

            if (connection.RemoteIpAddress != null) {
                if (connection.LocalIpAddress != null)
                {
                    if (!connection.RemoteIpAddress.Equals(connection.LocalIpAddress)) {
                        context.Result = new UnauthorizedObjectResult(new { code = 401, message = "Unauthorized Access. Remote access is not allowed." });
                        this._logger.LogError("Remote Access is not Allowed.");
                        return;
                    }
                }
                else {
                    if (!IPAddress.IsLoopback(connection.RemoteIpAddress)) {
                        context.Result = new UnauthorizedObjectResult(new { code = 401, message = "Unauthorized Access. Remote access is not allowed." });
                        this._logger.LogError("Remote Access is not Allowed.");
                        return;
                    }
                }
            }



            await next();
        }

        
    }
}

