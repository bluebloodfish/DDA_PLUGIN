﻿using DDAApi.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.Security
{
    public class AuthenFilter : IAsyncActionFilter
    {
        private const ulong _Max_Age_In_Seconds = 300;
        private static readonly DateTime _1970 = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthenFilter> _logger;
        private string _appId;
        private string _secretKey;
        private IHostingEnvironment _env;
        private readonly DDAApiSetting _options;

        public IConfiguration _config { get; }

        private Dictionary<string, string> KeyPairs = new Dictionary<string, string>();

        public AuthenFilter(ILogger<AuthenFilter> logger, IMemoryCache memoryCache, IConfiguration  config, IOptions<DDAApiSetting> options, IHostingEnvironment env)
        {
            this._cache = memoryCache;
            this._logger = logger;
            this._config = config;

            //this._appId = this._config["AppId"];
            //this._secretKey = this._config["SecretKey"];
            this._options = options.Value;
            this._appId = this._options.AppId;
            this._secretKey = this._options.SecretKey;
            this._env = env;
            //if (string.IsNullOrEmpty(this._appId) || string.IsNullOrEmpty(this._secretKey)) {
            //    this._logger.LogError("Missing AppId or SecretKey in config file.");
            //}
            
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var requestMethod = context.HttpContext.Request.Method;
            var requestPath = context.HttpContext.Request.Path.ToString().ToLower();

            if (!context.HttpContext.Request.Headers.TryGetValue("AppId", out var AppId))
            {
                context.Result = new UnauthorizedResult();
                this._logger.LogError("Missing AppId in header.");
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue("Sign", out var Sign))
            {
                context.Result = new UnauthorizedResult();
                this._logger.LogError("Missing Sign in header.");
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue("Nonce", out var Nonce))
            {
                context.Result = new UnauthorizedResult();
                this._logger.LogError("Missing Nonce in header.");
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue("Timestamp", out var TimeStamp))
            {
                context.Result = new UnauthorizedResult();
                this._logger.LogError("Missing Timestamp in header.");
                return;
            }

            

            if (IsReplayRequest(Nonce, TimeStamp))
            {
                context.Result = new UnauthorizedResult();
                this._logger.LogError("Detect replay request!!");
                return;
            }

            if (this._appId != AppId) {
                context.Result = new UnauthorizedResult();
                this._logger.LogError("Invalid AppId.");
                return;
            }


            List<string> list = new List<string>();

            list.Add(requestMethod);
            list.Add(requestPath);
            list.Add(AppId);
            list.Add(this._secretKey);
            list.Add(TimeStamp);
            list.Add(Nonce);


            list.Sort(StringComparer.Ordinal);
            var joinedStr = string.Join("&", list);
           

            string signature = CalculateMD5Hash(joinedStr);

            if (signature != Sign)
            {
                context.Result = new UnauthorizedResult();
                this._logger.LogError("Invalid signature!!");
                return;
            }

            await next();
        }

        private bool IsReplayRequest(string nonce, string requestTimeStamp)

        {
            //if (this._env.IsProduction())
            //{
            //    if (_cache.TryGetValue(nonce, out object _)) return true;
            //    TimeSpan currentTs = DateTime.UtcNow - _1970;
            //    var serverTotalSeconds = Convert.ToUInt64(currentTs.TotalSeconds);
            //    var requestTotalSeconds = Convert.ToUInt64(requestTimeStamp);
            //    this._logger.LogInformation($"requestTimeStamp: {requestTimeStamp}  ----- Now: {serverTotalSeconds}");

            //    if (serverTotalSeconds - requestTotalSeconds > _Max_Age_In_Seconds) return true;

            //    _cache.Set(nonce, requestTimeStamp, DateTimeOffset.UtcNow.AddSeconds(_Max_Age_In_Seconds));
            //    return false;
            //}
            //else {
            //    return false;
            //}

            return false;
        }


        private string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
