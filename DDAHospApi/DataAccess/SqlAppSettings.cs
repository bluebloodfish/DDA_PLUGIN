using DDAApi.HospModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.DataAccess
{
    public class SqlAppSettings : ISqAppSettings
    {
        private readonly AppDbContext _ctx;

        public SqlAppSettings(AppDbContext ctx)
        {
            this._ctx = ctx;
        }

        public Task<int> AddSqHttpCallback(string baseUrl)
        {
            var a = this._ctx.TT_ApiSettings.OrderBy(x=>x.Id).FirstOrDefault();
            if (a != null)
            {
                a.HttpBaseUrl = baseUrl;
                this._ctx.TT_ApiSettings.Update(a);
            }
            else
            {
                this._ctx.TT_ApiSettings.Add(new TT_ApiSetting { HttpBaseUrl = baseUrl, OnlineOrderStartYear = -1 });
            }
            //this._ctx.TT_ApiSetting.Update(a);
            return this._ctx.SaveChangesAsync();
        }

        public int GetOnlineOrderStartYear()
        {
            return this._ctx.TT_ApiSettings.OrderBy(x => x.Id).FirstOrDefault().OnlineOrderStartYear;
        }

        public Task<int> SetOnlineOrderStartYear(int year)
        {
            var settings = this._ctx.TT_ApiSettings.OrderBy(x => x.Id).FirstOrDefault();
            settings.OnlineOrderStartYear = year;
            this._ctx.TT_ApiSettings.Update(settings);
            return this._ctx.SaveChangesAsync();
        }
    }
}
