using DDAApi.DataAccess;
using DDAApi.TTOpenApi;
using DDAApi.Utility;
using DDAApi.WebApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DDAApi.DBServices
{
    public class DatabseCheckManager : IDatabseCheckManager
    {
        private readonly ILogger<DatabseCheckManager> _logger;
        private readonly IServiceProvider _serviceProvoider;
        private readonly AppDbContext _ctx;
        private readonly DDAApiSetting _options;
        private bool _isRunning = false;
        private bool _tryStop = false;
        private Thread _thread;

        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="logger"></param>
        public DatabseCheckManager(ILogger<DatabseCheckManager> logger, AppDbContext ctx, IOptions<DDAApiSetting> options)//IServiceProvider serviceProvider)
        {
            _logger = logger;
            _ctx = ctx;
            this._options = options.Value;
            //_serviceProvoider = serviceProvider;
        }

        /// <summary>
        /// 正在运行
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 启动队列
        /// </summary>
        public void Run()
        {
            if (_isRunning || (_thread != null && _thread.IsAlive))
            {
                _logger.LogWarning("DatabseCheck thread is running. New thread create is aborted.");
                return;
            }
            _isRunning = true;
            _thread = new Thread(StartSendOrder)
            {
                Name = "DatabaseCheck",
                IsBackground = true,
            };
            _logger.LogInformation("DatabseCheck thread is about to starting.");
            _thread.Start();
            _logger.LogInformation($"DatabseCheck is started，thread id is：{ _thread.ManagedThreadId}");
        }

        /// <summary>
        /// 停止队列
        /// </summary>
        public void Stop()
        {
            if (_tryStop)
            {
                return;
            }
            _tryStop = true;
        }

        private void StartSendOrder()
        {
            var sw = new Stopwatch();
            
           
            try
            {
                sw.Restart();
                SqlParameter outResult = new SqlParameter { Direction = System.Data.ParameterDirection.Output, SqlDbType = System.Data.SqlDbType.Int, ParameterName="@tcount" };
                //-----------------------------------------------------------------------------
                var isExistCmdStr = @"SELECT @tcount = count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'TT_OrderH_Log'";
                _ctx.Database.ExecuteSqlCommandAsync(isExistCmdStr, outResult).GetAwaiter().GetResult();

                if ((int)outResult.Value == 0)
                {
                    string createOrderHLogCmdString = @"CREATE TABLE [dbo].[TT_OrderH_Log](
                                                        [Id] [bigint] IDENTITY(1,1) NOT NULL, [OrderNo] [nchar](10) NULL,
                                                        [CreateDateTime] [datetime] NULL,     [ModifiedDateTime] [datetime] NULL,
                                                        [OpKind] [int] NULL,                  [OpName] [nvarchar](15) NULL,
                                                        [Status] [int] NULL,                  [TTOrderId] [nvarchar](50) NULL,
                                                        PRIMARY KEY (Id) )";
                    _ctx.Database.ExecuteSqlCommandAsync(createOrderHLogCmdString).GetAwaiter().GetResult();
                }


                isExistCmdStr = @"SELECT @tcount = count(*)FROM sys.objects WHERE[name] = N'TT_Tri_OrderH.PaidAmount' AND[type] = 'TR'";
                _ctx.Database.ExecuteSqlCommandAsync(isExistCmdStr, outResult).GetAwaiter().GetResult();
                if ((int)outResult.Value == 0)
                {
                    #region create trigger

                    //string cmdStr = @"EXEC sp_executeSQL N'Create trigger [TT_OrderH_PaidAmount] ON [OrderH] For Update As Begin SET NOCOUNT ON;"+
                    //                $"Declare @orderNo varchar(10), @opName nvarchar(15); Declare @oldAmount float, @newAmount float; Declare @ttOrderId nvarchar(8); Declare @url nvarchar(Max); select @ttOrderId = bookingno, @orderNo = orderno, @opName = opName, @newAmount = PaidAmount From inserted; If UPDATE(PaidAmount) and LEFT(@orderNo, 1) = ''#'' Begin Set @oldAmount = (select PaidAmount from deleted); if @newAmount <= 0 and @oldAmount > 0 begin INSERT INTO TT_OrderH_Log([OrderNo], TTOrderId, [CreateDateTime],[ModifiedDateTime],[OpKind],[OpName],[Status]) VALUES (@orderNo, @ttOrderId, GETDATE(), GETDATE(), 0, @opName, 0); Declare @dataJson nvarchar(Max); set @dataJson = ''{""id"": '' + @ttOrderId + ''}'' ; Select top 1 @url = [HttpBaseUrl] FROM [TT_HttpCallback]; set @url = @url + ''api_v1/HospOrder/cancelorder''; exec dbo.Fn_Http_Post @url, @dataJson, ""application/json"",""application/json""; end End End'";
                    #endregion
                    //this._logger.LogError(cmdStr);
                    //this._logger.LogError(cmdStr.Length.ToString());
                    //_ctx.Database.
                    //_ctx.Database.ExecuteSqlCommandAsync(cmdStr).GetAwaiter().GetResult();
                }


                //-------------------------------------------------------------------------------
                isExistCmdStr = @"SELECT @tcount = count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'TT_HttpCallback'";
                _ctx.Database.ExecuteSqlCommandAsync(isExistCmdStr, outResult).GetAwaiter().GetResult();

                if ((int)outResult.Value == 0)
                {
                    string createCmdStr = @"CREATE TABLE [dbo].[TT_HttpCallback]([HttpBaseUrl] [nvarchar](max) NULL) ";
                    _ctx.Database.ExecuteSqlCommandAsync(createCmdStr).GetAwaiter().GetResult();
                }

                isExistCmdStr = @"SELECT @tcount = count(*) FROM TT_HttpCallback";
                _ctx.Database.ExecuteSqlCommandAsync(isExistCmdStr, outResult).GetAwaiter().GetResult();
                var isEnableCancelOrderFunction = this._options.EnableCancelOrderFunction;

                if (isEnableCancelOrderFunction <= 0)
                {
                    if (((int)outResult.Value > 0))
                    {
                        string cmdStr = $"Delete from [dbo].[TT_HttpCallback]";
                        _ctx.Database.ExecuteSqlCommandAsync(cmdStr).GetAwaiter().GetResult();
                    }
                }
                else
                {
                    if ((int)outResult.Value == 0)
                    {
                        string insertCmdStr = $"INSERT INTO [dbo].[TT_HttpCallback] ([HttpBaseUrl]) VALUES ( '{this._options.SQLCallbackBaseUrl}' )";
                        _ctx.Database.ExecuteSqlCommandAsync(insertCmdStr).GetAwaiter().GetResult();
                    }
                    else
                    {
                        string updateCmdStr = $"UPDATE [dbo].[TT_HttpCallback] SET[HttpBaseUrl] = '{this._options.SQLCallbackBaseUrl}'";
                        _ctx.Database.ExecuteSqlCommandAsync(updateCmdStr).GetAwaiter().GetResult();
                    }
                }

                
                sw.Stop();
                _logger.LogInformation($"time elapsed: {sw.Elapsed.TotalSeconds}");    
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QueueManager Exception");
            }
             
        }

       



    }

   
}
