using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DDAApi.DataAccess;
using DDAApi.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DDAApi.Utility;
using DDAApi.OrderQueue;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using DDAApi.Order_Parser;
using Newtonsoft.Json.Serialization;
using DDAApi.TTOpenApi;
using DDAApi.DBServices;
using DDAApi.CancelOrderQueue;
using DDAApi.OrderNoQueue;

namespace DDAApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILogger<Startup> ilogger)
        {
            Configuration = configuration;
            _ilogger = ilogger;
        }

        public IConfiguration Configuration { get; }
        public ILogger<Startup> _ilogger { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<AppDbContext>(
                options => options.UseSqlServer(Configuration["DDADB"], builder => builder.UseRowNumberForPaging()));
            //services.AddDbContext<AppDbContext>(
            //        options => options.UseSqlServer(Configuration["DDADB"],
            //                                        sqlServerOptions => { sqlServerOptions.CommandTimeout(120); sqlServerOptions.UseRowNumberForPaging(); })
            //    );


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(
                    options => {
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    }
                );


            services.AddScoped<IHospMenuManager, HospMenuManager>();
            services.AddScoped<ITT_OrderProcess_Log_Manage, TT_OrderProcess_Log_Manage>();
            services.AddScoped<ITT_OrderNoMapping_Manage, TT_OrderNoMapping_Manage>();

            #region DDA Api Setting validation
            services.AddScoped<IHospOrderManage, HospOrderManage>();
            services.AddScoped<IHospTableManage, HospTableManage>();
            services.AddScoped<IDDAVersionManager, DDAVersionManager>();
            services.AddScoped<IProfileManager, ProfileManager>();
            services.AddScoped<IOrderHLogManager, OrderHLogManager>();
            services.AddScoped<ISqAppSettings, SqlAppSettings>();

            DDAApiSetting._hospMenuManager = services.BuildServiceProvider().GetService<IHospMenuManager>();
            DDAApiSetting._logger = services.BuildServiceProvider().GetService<ILogger<DDAApiSetting>>();
            //DDAApiSetting._sqlHttpCallback = services.BuildServiceProvider().GetService< ISqlHttpCallback>();

            //services.AddScoped<IStartupFilter, DatabaseCheckStartupFilter>();
            services.AddTransient<IStartupFilter, SettingValidationStartupFilter>();

            services.Configure<DDAApiSetting>(Configuration.GetSection(nameof(DDAApiSetting)));

            services.AddSingleton(resolver=> resolver.GetRequiredService<IOptions<DDAApiSetting>>().Value);

            services.AddSingleton<IValidatable>(resolver =>
                                resolver.GetRequiredService<IOptions<DDAApiSetting>>().Value);

            //services.AddScoped<IDatabseCheckManager, DatabseCheckManager>();
            //services.BuildServiceProvider().GetService<IDatabseCheckManager>().Run();

            #endregion

            services.AddTransient<IHospOrderParser, HospOrderParser>();
            services.AddTransient<IOrderProcessor, OrderProccessor>();
            services.AddScoped<AuthenFilter>();
            services.AddScoped<LocalRequestOnlyFilter>();
            services.AddTransient<ITT_OpenApi, TT_OpenApi>();

            services.AddSingleton<IOrderNoQueueProvider, OrderNoQueueProvider>();

            #region Job Queue
            services.AddSingleton<IOrderQueueProvider, OrderQueueProvider>();
            services.AddSingleton<IOrderQueueManager, OrderQueueManager>();
            services.AddSingleton<IOrderQueueService, OrderQueueService>();
            services.BuildServiceProvider().GetService<IOrderQueueManager>().Run();


            services.AddSingleton<ICancelOrderQueueProvider, CancelOrderQueueProvider>();
            services.AddSingleton<ICancelOrderQueueManager, CancelOrderQueueManager>();
            services.AddSingleton<ICancelOrderQueueService, CancelOrderQueueService>();


            var optionsDDA = services.BuildServiceProvider().GetService<IOptions<DDAApiSetting>>();
            if (optionsDDA.Value.EnableCancelOrderFunction > 0) {
                services.BuildServiceProvider().GetService<ICancelOrderQueueManager>().Run();
            }

            //services.AddMvc(options => { options.EnableEndpointRouting = false; });

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, AppDbContext dbContect, 
                        IOptions<DDAApiSetting> options, ISqAppSettings sqlAppSettings, IOrderNoQueueProvider orderNoQueueProvider, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else if (env.IsStaging()) {

            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            dbContect.Database.Migrate();
            //var orderNoParts = GetOrderNoParts(dbContect);
            //orderNoQueueProvider.ChangeSeed(orderNoParts);


            //Tim: if CancelOrder function is not activated, set SQLCallback Url empty.
            if (options.Value.EnableCancelOrderFunction <= 0)
            {
                sqlAppSettings.AddSqHttpCallback("").GetAwaiter().GetResult();
            }
            else
            {
                sqlAppSettings.AddSqHttpCallback(options.Value.SQLCallbackBaseUrl).GetAwaiter().GetResult();
            }

            var startYear = sqlAppSettings.GetOnlineOrderStartYear();
            if (startYear == -1)
            {
                sqlAppSettings.SetOnlineOrderStartYear(DateTime.Now.Year);
                OrderNoQueueProvider.StartYear = DateTime.Now.Year;
            }
            else {
                OrderNoQueueProvider.StartYear = startYear;
            }

            var orderNoParts = orderNoQueueProvider.InitOrderNoParts(DateTime.Now);
            orderNoQueueProvider.ChangeSeed(orderNoParts);
            //var databaseCreator = (dbContect.Database.getservice .GetService<IDatabaseCreator>() as RelationalDatabaseCreator);
            //databaseCreator.CreateTables();

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
            
        }

        //private OrderNoParts GetOrderNoParts(AppDbContext _ctx)
        //{

        //    OrderNoDatePair pair = new OrderNoDatePair();
        //    pair = _ctx.HospOrderHeads.AsNoTracking()
        //    .Where(x => x.OrderNo.StartsWith("#"))
        //    .Select(x => new OrderNoDatePair { MaxOrderNo = x.OrderNo, OrderDateTime = x.OrderDate })
        //    .OrderByDescending(x => x.MaxOrderNo).FirstOrDefault();
            
        //    if (pair == null)
        //    {
        //        return new OrderNoParts
        //        {
        //            YearLetter = 'A',
        //            CurrentMaxFixNo = 0,
        //            GenerateDate = DateTime.Now.Date
        //        };
        //    }
        //    else
        //    {
        //        char yearLetter = pair.MaxOrderNo[1];

        //        if (DateTime.Now.Date.Equals(pair.OrderDateTime.Date))
        //        {
        //            int no = int.Parse(pair.MaxOrderNo.Substring(6));
        //            return new OrderNoParts
        //            {
        //                YearLetter = yearLetter,
        //                CurrentMaxFixNo = no,
        //                GenerateDate = DateTime.Now.Date
        //            };
        //        }
        //        else
        //        {
        //            if (DateTime.Now.Year.CompareTo(pair.OrderDateTime.Year) > 0)
        //            {
        //                yearLetter++;
        //            }

        //            return new OrderNoParts
        //            {
        //                YearLetter = yearLetter,
        //                CurrentMaxFixNo = 0,
        //                GenerateDate = DateTime.Now.Date
        //            };
        //        }
        //    }
        //}
    }
}
